using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Common.Configuration;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class DeleteWebhookRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();
        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>();
        private readonly Mock<IEntityToDtoMapper> _entityToDtoMapper = new Mock<IEntityToDtoMapper>();

        private static OidcAuthenticationEntity _authentication = new OidcAuthenticationEntity(
                "captain-hook-id",
                "kv-secret-name",
                "https://blah-blah.sts.eshopworld.com",
                new[] { "scope1" });

        private static readonly SubscriberBuilder _subscriberBuilder = new SubscriberBuilder()
               .WithEvent("event")
               .WithName("subscriber")
               .WithWebhooksSelectionRule("$.Test")
               .WithWebhook("https://blah.blah.eshopworld.com/default/", "POST", null, authentication: _authentication)
               .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: _authentication)
               .WithWebhook("https://blah.blah.eshopworld.com/other-webhook/", "POST", "non-deletable", authentication: _authentication);

        private static readonly OidcAuthenticationDto _authenticationDto = new OidcAuthenticationDto
        {
            ClientId = "captain-hook-id",
            ClientSecretKeyName = "kv-secret-name",
            Uri = "https://blah-blah.sts.eshopworld.com",
            Scopes = new List<string> { "scope1" }
        };

        private static readonly EndpointDto _firstEndpoint = new EndpointDto
        {
            Selector = null,
            Uri = "https://blah.blah.eshopworld.com/default/",
            HttpVerb = "POST",
            Authentication = _authenticationDto
        };
        
        private static readonly EndpointDto _secondEndpoint = new EndpointDto
        {
            Selector = "selector",
            Uri = "https://blah.blah.eshopworld.com/webhook/",
            HttpVerb = "POST",
            Authentication = _authenticationDto
        };

        private static readonly EndpointDto _thirdEndpoint = new EndpointDto
        {
            Selector = "non-deletable",
            Uri = "https://blah.blah.eshopworld.com/other-webhook/",
            HttpVerb = "POST",
            Authentication = _authenticationDto
        };

        private readonly DeleteWebhookRequest _defaultRequest = new DeleteWebhookRequest("event", "subscriber", "selector");

        public DeleteWebhookRequestHandlerTests()
        {
            // SubscriberEntity returned from Repository.Get must be the same instance as returned from Repository.Update to preserve changes in endpoints applied during tests
            var subscriber = _subscriberBuilder.Create();
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
               .ReturnsAsync(subscriber);
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberConfiguration());
            _repositoryMock.Setup(x => x.UpdateSubscriberAsync(It.Is<SubscriberEntity>(x => x.Id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriber);            
        }

        private DeleteWebhookRequestHandler Handler => new DeleteWebhookRequestHandler(
            _repositoryMock.Object,
            _directorServiceMock.Object,
            _entityToDtoMapper.Object,
            new[]
            {
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(10.0)
            });

        [Fact, IsUnit]
        public async Task When_EndpointCanBeDeleted_Then_ChangesWillBeAppliedByDirectorServiceAndStoredInRepository()
        {
            _entityToDtoMapper.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .Returns(new SubscriberDto
                {
                    Webhooks = new WebhooksDto
                    {
                        SelectionRule = "$.Test",
                        Endpoints = new List<EndpointDto> { _firstEndpoint, _thirdEndpoint }
                    }
                });

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
            _entityToDtoMapper.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .Returns(new SubscriberDto
                {
                    Webhooks = new WebhooksDto
                    {
                        SelectionRule = "$.Test",
                        Endpoints = new List<EndpointDto> { _secondEndpoint, _thirdEndpoint }
                    }
                });

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
            _entityToDtoMapper.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .Returns(new SubscriberDto
                {
                    Webhooks = new WebhooksDto
                    {
                        SelectionRule = "$.Test",
                        Endpoints = new List<EndpointDto> { _firstEndpoint, _secondEndpoint, _thirdEndpoint }
                    }
                });

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
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
               .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));

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
               .WithWebhooksSelectionRule("$.Test")
               .WithWebhook("https://blah.blah.eshopworld.com/webhook/", "POST", "selector", authentication: _authentication)
               .Create();
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _repositoryMock.Setup(x => x.UpdateSubscriberAsync(It.Is<SubscriberEntity>(entity => entity.Webhooks.Endpoints.Count() == 1)))
                .ReturnsAsync(subscriberEntity);
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new SubscriberConfiguration());

            _entityToDtoMapper.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>()))
                .Returns(new SubscriberDto
                {
                    Webhooks = new WebhooksDto
                    {
                        SelectionRule = "$.Test",
                        Endpoints = new List<EndpointDto> { _thirdEndpoint }
                    }
                });

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CannotRemoveLastEndpointFromSubscriberError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new DirectorServiceIsBusyError());

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
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderCreateError("subscriber", "event"));

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
            _directorServiceMock.Setup(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderDeleteError("subscriber", "event"));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsSavingSubscriberAfterSuccessfulReaderCreation_Then_ErrorIsReturned()
        {
            _repositoryMock.Setup(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new BusinessError("test error"));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotUpdateEntityError_Then_OperationIsRetried3Times()
        {
            // here we need new instance every time
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
              .ReturnsAsync(() => _subscriberBuilder.Create());
            _repositoryMock.Setup(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new CannotUpdateEntityError("dummy-type", new Exception()));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            result.Should().BeEquivalentTo(new OperationResult<EndpointDto>(new CannotUpdateEntityError("dummy-type", new Exception())));
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
            _directorServiceMock.Verify(x => x.UpdateReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _repositoryMock.Verify(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotUpdateEntityErrorForFirstAttempt_Then_SucceedsOnSecondTry()
        {
            // here we need new instance every time
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
              .ReturnsAsync(() => _subscriberBuilder.Create());
            _repositoryMock.SetupSequence(x => x.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
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
