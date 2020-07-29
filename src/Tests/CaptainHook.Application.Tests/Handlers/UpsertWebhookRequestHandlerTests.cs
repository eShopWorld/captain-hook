using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Common.Remoting;
using CaptainHook.Common.Remoting.Types;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers
{
    public class UpsertWebhookRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();
        private readonly Mock<IDirectorServiceRemoting> _directorServiceMock = new Mock<IDirectorServiceRemoting>();
        private readonly Mock<ISecretProvider> _secretProviderMock = new Mock<ISecretProvider>();

        private UpsertWebhookRequestHandler handler => new UpsertWebhookRequestHandler(_repositoryMock.Object, _directorServiceMock.Object, _secretProviderMock.Object);

        [Fact, IsUnit]
        public async Task When_Subscriber_does_not_exist_then_new_one_will_be_passed_to_DirectorService_and_stored_in_Repository()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeFalse();
            _repositoryMock.VerifyAll();
            _directorServiceMock.VerifyAll();
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_does_exist_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var subscriberEntity = new SubscriberBuilder()
                .WithEvent("event")
                .WithName("subscriber")
                .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
                    new AuthenticationEntity(
                        "captain-hook-id",
                        new SecretStoreEntity("kvname", "kv-secret-name"),
                        "https://blah-blah.sts.eshopworld.com",
                        "OIDC",
                        new[] { "scope1" })
                ).Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);

            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 2)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_retrieving_Subscriber_data_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new BusinessError("test error"));
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_throws_an_Exception_retrieving_Subscriber_data_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .Throws<Exception>();
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Never);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_is_busy_reloading_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_throws_an_Exception_during_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                 .Throws<Exception>();

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_saving_Subscriber_after_successfull_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new BusinessError("test error"));
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_Repository_throws_an_Exception_saving_Subscriber_after_successfull_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .Throws<Exception>();
            _directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        private bool MatchReaderChangeInfo(in ReaderChangeInfo rci, UpsertWebhookRequest request)
        {
            var config = rci.NewReader.SubscriberConfig;

            return config.SubscriberName == request.SubscriberName
                   && config.EventType == request.EventName
                   && config.Name == $"{request.EventName}-{request.SubscriberName}"
                   && config.Uri == request.Endpoint.Uri
                   && config.AuthenticationConfig.Type.ToString().Equals(request.Endpoint.Authentication.Type, StringComparison.CurrentCultureIgnoreCase);
            // TODO: extend auth config comparison?
        }
    }
}
