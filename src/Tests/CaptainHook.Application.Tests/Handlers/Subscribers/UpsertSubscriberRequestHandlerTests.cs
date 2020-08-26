using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class UpsertSubscriberRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>(MockBehavior.Strict);

        private readonly Mock<IDirectorServiceProxy> _directorServiceMock = new Mock<IDirectorServiceProxy>(MockBehavior.Strict);

        private readonly UpsertSubscriberRequest _testRequest = new UpsertSubscriberRequest("event", "subscriber", new SubscriberDtoBuilder().Create());

        private readonly UpsertSubscriberRequestHandler _handler;

        public UpsertSubscriberRequestHandlerTests()
        {
            _handler = new UpsertSubscriberRequestHandler(_repositoryMock.Object, _directorServiceMock.Object);
        }

        [Fact, IsUnit]
        public async Task When_SubscriberDoesNotExist_Then_NewOneWillBePassedToDirectorServiceAndStoredInRepository()
        {
            var endpointDto = new EndpointDtoBuilder()
                .With(x => x.Uri, "https://blah-{selector}.eshopworld.com/webhook/")
                .Create();
            var subscriberDto = new SubscriberDtoBuilder()
                .With(s => s.Webhooks,
                    new WebhooksDto { SelectionRule = "$.TestRule", Endpoints = new List<EndpointDto> { endpointDto } })
                .Create();
            var request = new UpsertSubscriberRequest("event", "subscriber", subscriberDto);
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.IsAny<SubscriberId>()))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberEntity("subscriber"));

            var result = await _handler.Handle(request, CancellationToken.None);

            var expectedResult = new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(subscriberDto, UpsertType.Created));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_Subscriber_DoesExist_Then_SubscriberIsUpdated()
        {
            var subscriberEntity = new SubscriberBuilder()
                .WithEvent("event")
                .WithName("subscriber")
                .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "selector",
                    authentication: new OidcAuthenticationEntity(
                        "captain-hook-id",
                        "kv-secret-name",
                        "https://blah-blah.sts.eshopworld.com",
                        new[] { "scope1" })
                ).Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _directorServiceMock.Setup(r => r.UpdateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.UpdateSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new SubscriberEntity("subscriber"));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<UpsertResult<SubscriberDto>>(new UpsertResult<SubscriberDto>(_testRequest.Subscriber, UpsertType.Updated));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsRetrievingSubscriberData_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new BusinessError("test error"));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new BusinessError("test error"));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceIsBusyReloading_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new DirectorServiceIsBusyError());
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToRefreshReaderAsync_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new ReaderCreateError(new SubscriberEntity("subscriber")));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new ReaderCreateError(new SubscriberEntity("subscriber")));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsSavingSubscriberAfterSuccessfulReaderCreation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _directorServiceMock.Setup(r => r.CreateReaderAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(true);
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new BusinessError("test error"));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new BusinessError("test error"));
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}