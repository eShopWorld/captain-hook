using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Requests.Subscribers;
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
        [Fact, IsUnit]
        public async Task When_Subscriber_does_not_exist_then_new_one_will_be_passed_to_DirectorService_and_stored_in_Repository()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeFalse();
            repositoryMock.VerifyAll();
            directorServiceMock.VerifyAll();
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_does_exist_then_will_be_updated_with_new_WebHook_in_repository()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            var subscriberEntity = new SubscriberBuilder()
                .WithEvent("event")
                .WithName("subscriber")
                .WithWebhook(
                    "https://blah.blah.eshopworld.com/oldwebhook/",
                    "POST",
                    "selector",
                    new AuthenticationEntity(
                        "captain-hook-id",
                        new SecretStoreEntity("kvname", "kv-secret-name"),
                        "https://blah-blah.sts.eshopworld.com",
                        "OIDC",
                        new[] { "scope1" })
                ).Create();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);

            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 2)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeFalse();
            repositoryMock.VerifyAll();
            directorServiceMock.VerifyAll();
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_retrieving_Subscriber_data_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new BusinessError("test error"));
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Never);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_throws_an_Exception_retrieving_Subscriber_data_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .Throws<Exception>();
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadStarted);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Never);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_is_busy_reloading_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_throws_an_Exception_during_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(Guid.NewGuid());
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                 .Throws<Exception>();

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_saving_Subscriber_after_successfull_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new BusinessError("test error"));
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_Repository_throws_an_Exception_saving_Subscriber_after_successfull_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            var repositoryMock = new Mock<ISubscriberRepository>();
            var directorServiceMock = new Mock<IDirectorServiceRemoting>();

            repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .Throws<Exception>();
            directorServiceMock.Setup(r => r.UpdateReader(It.Is<ReaderChangeInfo>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(RequestReloadConfigurationResult.ReloadInProgress);

            var handler = new UpsertWebhookRequestHandler(repositoryMock.Object, directorServiceMock.Object);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            directorServiceMock.Verify(x => x.UpdateReader(It.IsAny<ReaderChangeInfo>()), Times.Once);
            repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
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
