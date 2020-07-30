using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Gateways;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Requests.Subscribers;
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
        private readonly Mock<IDirectorServiceGateway> _directorServiceMock = new Mock<IDirectorServiceGateway>();

        private UpsertWebhookRequestHandler handler => new UpsertWebhookRequestHandler(_repositoryMock.Object, _directorServiceMock.Object);

        [Fact, IsUnit]
        public async Task When_Subscriber_does_not_exist_then_new_one_will_be_passed_to_DirectorService_and_stored_in_Repository()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(true);

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
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(false);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Never);
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
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(false);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Never);
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
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(false);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Never);
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
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorService_fails_to_create_Reader_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(new ReaderCreationError(new SubscriberEntity("subscriber")));

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreationError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Once);
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
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                 .Throws<Exception>();

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_Repository_fails_saving_Subscriber_after_successful_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new BusinessError("test error"));
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(true);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_Repository_throws_an_Exception_saving_Subscriber_after_successful_Reader_creation_then_operation_fails()
        {
            var request = new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().Create());

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync((SubscriberEntity)null);
            _repositoryMock.Setup(r => r.SaveSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .Throws<Exception>();
            _directorServiceMock.Setup(r => r.CreateReader(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, request))))
                .ReturnsAsync(false);

            var result = await handler.Handle(request, CancellationToken.None);

            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReader(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.SaveSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        private bool MatchReaderChangeInfo(in SubscriberEntity entity, UpsertWebhookRequest request)
        {
            return entity.Name == request.SubscriberName
                   && entity.ParentEvent.Name == request.EventName
                   && entity.Webhooks.Endpoints.First().Uri == request.Endpoint.Uri
                   && entity.Webhooks.Endpoints.First().Authentication.Type.Equals(request.Endpoint.Authentication.Type, StringComparison.CurrentCultureIgnoreCase);
            // TODO: extend auth config comparison?
        }
    }
}
