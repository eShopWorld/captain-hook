using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class UpsertWebhookRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();

        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>();

        private readonly UpsertWebhookRequest _defaultUpsertRequest =
            new UpsertWebhookRequest("event", "subscriber", new EndpointDtoBuilder().With(x => x.Selector, null).Create());

        private UpsertWebhookRequestHandler Handler => new UpsertWebhookRequestHandler(
            _repositoryMock.Object,
            _directorServiceMock.Object, 
            Mock.Of<IBigBrother>(),
            new[]
            {
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(10.0)
            });

        [Fact, IsUnit]
        public async Task When_SubscriberDoesNotExist_Then_NewOneWillBePassedToDirectorServiceAndStoredInRepository()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(true);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            _repositoryMock.VerifyAll();
            _directorServiceMock.VerifyAll();
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_DoesExist_Then_SameEndpointReturned()
        {
            var subscriberEntity = new SubscriberBuilder()
                .WithEvent("event")
                .WithName("subscriber")
                .WithWebhookSelectionRule("$.Test")
                .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "abc", 
                    authentication: new AuthenticationEntity(
                        "captain-hook-id",
                        new SecretStoreEntity("kvname", "kv-secret-name"),
                        "https://blah-blah.sts.eshopworld.com",
                        "OIDC",
                        new[] { "scope1" })
                ).Create();
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberEntity("subscriber"));

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            result.Data.Should().BeEquivalentTo(_defaultUpsertRequest.Endpoint);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsRetrievingSubscriberData_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new BusinessError("test error"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(false);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryThrowsAnExceptionRetrievingSubscriberData_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .Throws<Exception>();
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(false);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToRefreshReaderAsync_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(new ReaderCreateError(new SubscriberEntity("subscriber")));

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceThrowsAnExceptionDuringReaderCreation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                 .Throws<Exception>();

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsSavingSubscriberAfterSuccessfulReaderCreation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(new BusinessError("test error"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(true);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryThrowsAnExceptionSavingSubscriberAfterSuccessfulReaderCreation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .Throws<Exception>();
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultUpsertRequest))))
                .ReturnsAsync(false);

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFails_Then_OperationIsRetried3Times()
        {
            SubscriberEntity SubscriberEntityFunc() =>
                new SubscriberBuilder().WithEvent("event")
                    .WithName("subscriber")
                    .WithWebhookSelectionRule("$.Test")
                    .WithWebhook(
                        "https://blah.blah.eshopworld.com/oldwebhook/",
                        "POST",
                        "abc",
                        authentication: new AuthenticationEntity(
                            "captain-hook-id",
                            new SecretStoreEntity("kvname", "kv-secret-name"),
                            "https://blah-blah.sts.eshopworld.com",
                            "OIDC",
                            new[] { "scope1" }))
                    .Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => SubscriberEntityFunc());
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()));

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(new CannotUpdateEntityError("dummy-type", new Exception()));
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFails_Then_SucceedsOnSecondTry()
        {
            SubscriberEntity SubscriberEntityFunc() =>
                new SubscriberBuilder().WithEvent("event")
                    .WithName("subscriber")
                    .WithWebhookSelectionRule("$.Test")
                    .WithWebhook(
                        "https://blah.blah.eshopworld.com/oldwebhook/",
                        "POST",
                        "abc",
                        authentication: new AuthenticationEntity(
                            "captain-hook-id",
                            new SecretStoreEntity("kvname", "kv-secret-name"),
                            "https://blah-blah.sts.eshopworld.com",
                            "OIDC",
                            new[] { "scope1" }))
                    .Create();
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => SubscriberEntityFunc());
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.SetupSequence(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()))
                .ReturnsAsync(SubscriberEntityFunc());

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(_defaultUpsertRequest.Endpoint);
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
        }

        [Fact, IsUnit]
        public async Task When_DocumentUpdateFailsAndDocumentIsRemoved_Then_TriesToAddOnSecondTry()
        {
            SubscriberEntity SubscriberEntityFunc() =>
                new SubscriberBuilder().WithEvent("event")
                    .WithName("subscriber")
                    .WithWebhookSelectionRule("$.Test")
                    .WithWebhook(
                        "https://blah.blah.eshopworld.com/oldwebhook/",
                        "POST",
                        "abc",
                        authentication: new AuthenticationEntity(
                            "captain-hook-id",
                            new SecretStoreEntity("kvname", "kv-secret-name"),
                            "https://blah-blah.sts.eshopworld.com",
                            "OIDC",
                            new[] { "scope1" }))
                    .Create();
            _repositoryMock.SetupSequence(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => SubscriberEntityFunc())
                .ReturnsAsync(new EntityNotFoundError("dummy-type", "dummy-key"));
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.SetupSequence(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()))
                .ReturnsAsync(SubscriberEntityFunc());
            _directorServiceMock.Setup(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(SubscriberEntityFunc());

            var result = await Handler.Handle(_defaultUpsertRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(_defaultUpsertRequest.Endpoint);
            result.Should().BeEquivalentTo(expectedResult);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _directorServiceMock.Verify(x => x.CreateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.AddSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        private bool MatchReaderChangeInfo(in SubscriberEntity entity, UpsertWebhookRequest request)
        {
            var endpointEntity = entity.Webhooks.Endpoints.First();

            return entity.Name == request.SubscriberName
                   && entity.ParentEvent.Name == request.EventName
                   && endpointEntity.Uri == request.Endpoint.Uri
                   && MatchUriTransforms(request.Endpoint.UriTransform, endpointEntity.UriTransform)
                   && endpointEntity.Authentication.Type.Equals(request.Endpoint.Authentication.Type, StringComparison.CurrentCultureIgnoreCase);
        }

        private static bool MatchUriTransforms(UriTransformDto dto, UriTransformEntity entity)
        {
            if (dto == null && entity == null)
                return true;

            if (dto.Replace.Count != entity.Replace.Count)
                return false;

            return dto.Replace.SequenceEqual(entity.Replace);
        }
    }
}
