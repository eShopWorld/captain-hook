using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
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
    public class DeleteSubscriberRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>();
        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>();
        private readonly Mock<IEntityToDtoMapper> _entityToDtoMapperMock = new Mock<IEntityToDtoMapper>();

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

        private static readonly SubscriberDto _subscriberDto = new SubscriberDto
        {
            Webhooks = new WebhooksDto
            {
                SelectionRule = "$.Test",
                Endpoints = new List<EndpointDto>
                {
                    new EndpointDto
                    {
                        Selector = null,
                        Uri = "https://blah.blah.eshopworld.com/default/",
                        HttpVerb = "POST",
                    }
                }
            }
        };

        private readonly DeleteSubscriberRequest _defaultRequest = new DeleteSubscriberRequest("event", "subscriber");

        public DeleteSubscriberRequestHandlerTests()
        {
            // SubscriberEntity returned from Repository.Get must be the same instance as returned from Repository.Update to preserve changes in endpoints applied during tests
            var subscriber = _subscriberBuilder.Create();
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
               .ReturnsAsync(subscriber);
            _directorServiceMock.Setup(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new List<SubscriberEntity>());
            _repositoryMock.Setup(x => x.RemoveSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new SubscriberId("event", "subscriber"));
            _entityToDtoMapperMock.Setup(x => x.MapSubscriber(It.IsAny<SubscriberEntity>())).Returns(_subscriberDto);
        }

        private DeleteSubscriberRequestHandler Handler => new DeleteSubscriberRequestHandler(
            _repositoryMock.Object,
            _directorServiceMock.Object,
             _entityToDtoMapperMock.Object,
            new[]
            {
                TimeSpan.FromMilliseconds(10.0),
                TimeSpan.FromMilliseconds(10.0)
            });

        [Fact, IsUnit]
        public async Task When_EndpointCanBeDeleted_Then_ChangesWillBeAppliedByDirectorServiceAndStoredInRepository()
        {
            var result = await Handler.Handle(new DeleteSubscriberRequest("event", "subscriber"), CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeFalse();
            result.Data.Should().BeEquivalentTo(_subscriberDto);

            var subscriberId = new SubscriberId("event", "subscriber");

            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(subscriberId))), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(subscriberId))), Times.Once);
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
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Never);
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            _directorServiceMock.Setup(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>())).ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<DirectorServiceIsBusyError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToUpdateReaderBecauseOfDeleteError_Then_OperationFails()
        {
            _directorServiceMock.Setup(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderDeleteError(new SubscriberEntity("subscriber")));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<ReaderDeleteError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Never);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsRemovingSubscriberAfterSuccessfulReaderDeletion_Then_ErrorIsReturned()
        {
            _repositoryMock.Setup(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()))
                .ReturnsAsync(new BusinessError("test error"));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            using var scope = new AssertionScope();
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<BusinessError>();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Once);
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Once);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotDeleteEntityError_Then_OperationIsRetried3Times()
        {
            // here we need new instance every time
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
              .ReturnsAsync(() => _subscriberBuilder.Create());
            _repositoryMock.Setup(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()))
                .ReturnsAsync(new CannotDeleteEntityError("dummy-type", new Exception()));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            result.Should().BeEquivalentTo(new OperationResult<SubscriberDto>(new CannotDeleteEntityError("dummy-type", new Exception())));
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(3));
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(3));
        }

        [Fact, IsUnit]
        public async Task When_RepositoryReturnsCannotDeleteEntityErrorForFirstAttempt_Then_SucceedsOnSecondTry()
        {
            // here we need new instance every time
            _repositoryMock.Setup(x => x.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
              .ReturnsAsync(() => _subscriberBuilder.Create());
            _repositoryMock.SetupSequence(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()))
                .ReturnsAsync(new CannotDeleteEntityError("dummy-type", new Exception()))
                .ReturnsAsync(new SubscriberId("event", "subscriber"));

            var result = await Handler.Handle(_defaultRequest, CancellationToken.None);

            result.IsError.Should().BeFalse();
            _repositoryMock.Verify(x => x.GetSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
            _directorServiceMock.Verify(x => x.DeleteReaderAsync(It.IsAny<SubscriberEntity>()), Times.Exactly(2));
            _repositoryMock.Verify(x => x.RemoveSubscriberAsync(It.IsAny<SubscriberId>()), Times.Exactly(2));
        }
    }
}
