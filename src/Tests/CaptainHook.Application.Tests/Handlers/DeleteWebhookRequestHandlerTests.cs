using System;
using System.Collections.Generic;
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

namespace CaptainHook.Application.Tests.Handlers
{
    public class DeleteWebhookRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();
        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>();

        private static AuthenticationEntity authentication = new AuthenticationEntity(
                "captain-hook-id",
                new SecretStoreEntity("kvname", "kv-secret-name"),
                "https://blah-blah.sts.eshopworld.com",
                "OIDC",
                new[] { "scope1" }
            );

        private readonly SubscriberBuilder _subscriberBuilder = new SubscriberBuilder()
               .WithEvent("event")
               .WithName("subscriber")
               .WithWebhookSelectionRule("$.Test")
               .WithWebhook("https://blah.blah.eshopworld.com/default/", "POST", null, authentication: authentication)
               .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: authentication)
               .WithWebhook("https://blah.blah.eshopworld.com/other-webhook/", "POST", "non-deletable", authentication: authentication);


        //private SubscriberEntity _defaultSubscriber = new SubscriberBuilder()
        //       .WithEvent("event")
        //       .WithName("subscriber")
        //       .WithWebhookSelectionRule("$.Test")
        //       .WithWebhook("https://blah.blah.eshopworld.com/default/", "POST", null, authentication: authentication)
        //       .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: authentication)
        //       .WithWebhook("https://blah.blah.eshopworld.com/other-webhook/", "POST", "non-deletable", authentication: authentication)
        //    .Create();

        private readonly DeleteWebhookRequest _defaultRequest = new DeleteWebhookRequest("event", "subscriber", "selector");

        public DeleteWebhookRequestHandlerTests()
        {
            //authentication = new AuthenticationEntity(
            //    "captain-hook-id",
            //    new SecretStoreEntity("kvname", "kv-secret-name"),
            //    "https://blah-blah.sts.eshopworld.com",
            //    "OIDC",
            //    new[] { "scope1" }
            //);

            //_defaultSubscriber = new SubscriberBuilder()
            //   .WithEvent("event")
            //   .WithName("subscriber")
            //   .WithWebhookSelectionRule("$.Test")
            //   .WithWebhook("https://blah.blah.eshopworld.com/default/", "POST", null, authentication: authentication)
            //   .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: authentication)
            //   .WithWebhook("https://blah.blah.eshopworld.com/other-webhook/", "POST", "non-deletable", authentication: authentication)
            //   .Create();

            var _defaultSubscriber = _subscriberBuilder.Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
               .ReturnsAsync(() => _subscriberBuilder.Create());
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(() => _subscriberBuilder.Create());
        }

        private DeleteWebhookRequestHandler Handler => new DeleteWebhookRequestHandler(
            _repositoryMock.Object,
            _directorServiceMock.Object,
            Mock.Of<IBigBrother>(),
            new[]
            {
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(10.0)
            });

        [Fact, IsUnit]
        public async Task When_EndpointCanBeDeleted_Then_ChangesWillBeAppliedByDirectorServiceAndStoredInRepository()
        {
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(_defaultSubscriber);
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
            //    .ReturnsAsync(true);
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));

            var result = await Handler.Handle(new DeleteWebhookRequest("event", "subscriber", "selector"), CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            result.Data.Webhooks.Endpoints.Should().HaveCount(2);
            result.Data.Webhooks.Endpoints.Should().OnlyContain(x => x.Selector != "selector");
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_DefaultEndpointCanBeDeleted_Then_ChangesWillBeAppliedByDirectorServiceAndStoredInRepository()
        {
            var result = await Handler.Handle(new DeleteWebhookRequest("event", "subscriber", null), CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            result.Data.Webhooks.Endpoints.Should().HaveCount(2);
            result.Data.Webhooks.Endpoints.Should().OnlyContain(x => x.Selector != null);
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_EndpointDoesNotExist_Then_EndpointNotFoundInSubscriberErrorReturned()
        {
            //var subscriberEntity =  new SubscriberBuilder()
            //   .WithEvent("event")
            //   .WithName("subscriber")
            //   .WithWebhookSelectionRule("$.Test")
            //   .WithWebhook("https://blah.blah.eshopworld.com/default/", "POST", null, authentication: authentication)
            //   .WithWebhook("https://blah.blah.eshopworld.com/other-webhook/", "POST", "non-deletable", authentication: authentication)
            //   .Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);

            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(subscriberEntity);
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(true);

            var result = await Handler.Handle(new DeleteWebhookRequest("event", "subscriber", "unknown-selector"), CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EndpointNotFoundInSubscriberError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_SubscriberDoesNotExist_Then_EntityNotFoundErrorReturned()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
               .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(true);

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<EntityNotFoundError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_SubscriberContainOnlyOneEndpoint_Then_CannotRemoveLastEndpointFromSubscriberErrorReturned()
        {
            var subscriberEntity = new SubscriberBuilder()
               .WithEvent("event")
               .WithName("subscriber")
               .WithWebhookSelectionRule("$.Test")
               .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: authentication)
               .Create();
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(subscriberEntity);
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(true);

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CannotRemoveLastEndpointFromSubscriberError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryThrowsAnExceptionRetrievingSubscriberData_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .Throws<Exception>();
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            //_directorServiceMock.Setup(r => r.CreateReaderAsync(It.Is<SubscriberEntity>(rci => MatchReaderChangeInfo(rci, _defaultRequest))))
            //    .ReturnsAsync(false);

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //    .WithEvent("event")
            //    .WithName("subscriber")
            //    .WithWebhookSelectionRule("$.Test")
            //    .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //        authentication: new AuthenticationEntity(
            //            "captain-hook-id",
            //            new SecretStoreEntity("kvname", "kv-secret-name"),
            //            "https://blah-blah.sts.eshopworld.com",
            //            "OIDC",
            //            new[] { "scope1" }
            //            )
            //    ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);

            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(s => s.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToUpdateReaderBecauseOfCreateError_Then_OperationFails()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //    .WithEvent("event")
            //    .WithName("subscriber")
            //    .WithWebhookSelectionRule("$.Test")
            //    .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //        authentication: new AuthenticationEntity(
            //            "captain-hook-id",
            //            new SecretStoreEntity("kvname", "kv-secret-name"),
            //            "https://blah-blah.sts.eshopworld.com",
            //            "OIDC",
            //            new[] { "scope1" }
            //            )
            //    ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);

            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderCreateError(new SubscriberEntity("subscriber")));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderCreateError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToUpdateReaderBecauseOfDeleteError_Then_OperationFails()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //    .WithEvent("event")
            //    .WithName("subscriber")
            //    .WithWebhookSelectionRule("$.Test")
            //    .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //        authentication: new AuthenticationEntity(
            //            "captain-hook-id",
            //            new SecretStoreEntity("kvname", "kv-secret-name"),
            //            "https://blah-blah.sts.eshopworld.com",
            //            "OIDC",
            //            new[] { "scope1" }
            //            )
            //    ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);

            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderDeleteError(new SubscriberEntity("subscriber")));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceThrowsAnExceptionDuringReaderUpdate_Then_UnhandledExceptionErrorReturned()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //    .WithEvent("event")
            //    .WithName("subscriber")
            //    .WithWebhookSelectionRule("$.Test")
            //    .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //        authentication: new AuthenticationEntity(
            //            "captain-hook-id",
            //            new SecretStoreEntity("kvname", "kv-secret-name"),
            //            "https://blah-blah.sts.eshopworld.com",
            //            "OIDC",
            //            new[] { "scope1" }
            //            )
            //    ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);
            //_repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
            //    .ReturnsAsync(new SubscriberEntity("subscriber"));
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).Throws<Exception>();

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsSavingSubscriberAfterSuccessfulReaderCreation_Then_ErrorIsReturned()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //     .WithEvent("event")
            //     .WithName("subscriber")
            //     .WithWebhookSelectionRule("$.Test")
            //     .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //         authentication: new AuthenticationEntity(
            //             "captain-hook-id",
            //             new SecretStoreEntity("kvname", "kv-secret-name"),
            //             "https://blah-blah.sts.eshopworld.com",
            //             "OIDC",
            //             new[] { "scope1" }
            //             )
            //     ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new BusinessError("test error"));
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(true);

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryThrowsAnExceptionSavingSubscriberAfterSuccessfulReaderCreation_Then_UnhandledExceptionErrorReturned()
        {
            //var subscriberEntity = new SubscriberBuilder()
            //     .WithEvent("event")
            //     .WithName("subscriber")
            //     .WithWebhookSelectionRule("$.Test")
            //     .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
            //         authentication: new AuthenticationEntity(
            //             "captain-hook-id",
            //             new SecretStoreEntity("kvname", "kv-secret-name"),
            //             "https://blah-blah.sts.eshopworld.com",
            //             "OIDC",
            //             new[] { "scope1" }
            //             )
            //     ).Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(subscriberEntity);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>())).ThrowsAsync(new Exception());
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(false);

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<UnhandledExceptionError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotUpdateEntityError_Then_OperationIsRetried3Times()
        {
            //SubscriberEntity SubscriberEntityFunc() =>
            //    new SubscriberBuilder().WithEvent("event")
            //        .WithName("subscriber")
            //        .WithWebhookSelectionRule("$.Test")
            //        .WithWebhook(
            //            "https://blah.blah.eshopworld.com/oldwebhook/",
            //            "POST",
            //            "abc",
            //            authentication: new AuthenticationEntity(
            //                "captain-hook-id",
            //                new SecretStoreEntity("kvname", "kv-secret-name"),
            //                "https://blah-blah.sts.eshopworld.com",
            //                "OIDC",
            //                new[] { "scope1" }))
            //        .Create();

            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(() => SubscriberEntityFunc());
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
            //    .ReturnsAsync(true);

            SubscriberEntity SubscriberEntityFunc() => _subscriberBuilder.Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(() => SubscriberEntityFunc());

            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            var expectedResult = new OperationResult<EndpointDto>(new CannotUpdateEntityError("dummy-type", new Exception()));
            result.Should().BeEquivalentTo(expectedResult);

            //result.Should().BeEquivalentTo(new CannotUpdateEntityError("dummy-type", new Exception()));
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotUpdateEntityErrorForFirstAttempt_Then_SucceedsOnSecondTry()
        {
            //SubscriberEntity SubscriberEntityFunc() =>
            //    new SubscriberBuilder().WithEvent("event")
            //        .WithName("subscriber")
            //        .WithWebhookSelectionRule("$.Test")
            //        .WithWebhook(
            //            "https://blah.blah.eshopworld.com/oldwebhook/",
            //            "POST",
            //            "abc",
            //            authentication: new AuthenticationEntity(
            //                "captain-hook-id",
            //                new SecretStoreEntity("kvname", "kv-secret-name"),
            //                "https://blah-blah.sts.eshopworld.com",
            //                "OIDC",
            //                new[] { "scope1" }))
            //        .Create();
            //_repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
            //    .ReturnsAsync(() => SubscriberEntityFunc());
            //_directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
            //    .ReturnsAsync(true);

            _repositoryMock.SetupSequence(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()))
                .ReturnsAsync(() => _subscriberBuilder.Create());

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            result.IsError.Should().BeFalse();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
        }
    }
}
