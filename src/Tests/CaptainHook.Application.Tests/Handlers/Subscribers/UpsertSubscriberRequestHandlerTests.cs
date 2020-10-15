using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Handlers.Subscribers;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
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
using Moq;
using Xunit;

namespace CaptainHook.Application.Tests.Handlers.Subscribers
{
    public class UpsertSubscriberRequestHandlerTests
    {
        private readonly Mock<ISubscriberRepository> _repositoryMock = new Mock<ISubscriberRepository>(MockBehavior.Strict);
        private readonly Mock<IDirectorServiceRequestsGenerator> _requestsGeneratorMock = new Mock<IDirectorServiceRequestsGenerator>(MockBehavior.Strict);
        private readonly Mock<IDtoToEntityMapper> _dtoToEntityMapper = new Mock<IDtoToEntityMapper>(MockBehavior.Strict);
        private readonly Mock<IDirectorServiceProxy> _directorServiceProxyMock = new Mock<IDirectorServiceProxy>(MockBehavior.Strict);

        private readonly UpsertSubscriberRequest _testRequest = new UpsertSubscriberRequest("event", "subscriber", new SubscriberDtoBuilder().Create());

        private readonly UpsertSubscriberRequestHandler _handler;

        public UpsertSubscriberRequestHandlerTests()
        {
            _handler = new UpsertSubscriberRequestHandler(_repositoryMock.Object, _requestsGeneratorMock.Object, _dtoToEntityMapper.Object, _directorServiceProxyMock.Object);

            _dtoToEntityMapper.Setup(r => r.MapWebooks(It.IsAny<WebhooksDto>(), WebhooksEntityType.Webhooks))
                .Returns(new WebhooksEntity(WebhooksEntityType.Webhooks));
            _dtoToEntityMapper.Setup(r => r.MapWebooks(It.IsAny<WebhooksDto>(), WebhooksEntityType.Callbacks))
                .Returns(new WebhooksEntity(WebhooksEntityType.Callbacks));
            _dtoToEntityMapper.Setup(r => r.MapWebooks(It.IsAny<WebhooksDto>(), WebhooksEntityType.DlqHooks))
               .Returns(new WebhooksEntity(WebhooksEntityType.DlqHooks));
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
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new[] { new UpdateReader() });
            _directorServiceProxyMock.Setup(r => r.CallDirectorServiceAsync(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(new SubscriberConfiguration());
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
                .WithWebhook("https://blah.blah.eshopworld.com/oldwebhook/", "POST", "*",
                    authentication: new OidcAuthenticationEntity(
                        "captain-hook-id",
                        "kv-secret-name",
                        "https://blah-blah.sts.eshopworld.com",
                        new[] { "scope1" })
                ).Create();

            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(subscriberEntity);
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
               .ReturnsAsync(new[] { new UpdateReader() });
            _directorServiceProxyMock.Setup(r => r.CallDirectorServiceAsync(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(new SubscriberConfiguration());
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
        public async Task When_DirectorServiceRequestGeneratorFails_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
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
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
               .ReturnsAsync(new[] { new UpdateReader() });
            _directorServiceProxyMock.Setup(r => r.CallDirectorServiceAsync(It.IsAny<UpdateReader>()))
                .ReturnsAsync(new DirectorServiceIsBusyError());

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new DirectorServiceIsBusyError());
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_DirectorServiceFailsToUpdateReaderAsync_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
               .ReturnsAsync(new[] { new UpdateReader() });
            _directorServiceProxyMock.Setup(r => r.CallDirectorServiceAsync(It.IsAny<UpdateReader>()))
                .ReturnsAsync(new ReaderCreateError("subscriber", "event"));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new ReaderCreateError("subscriber", "event"));
            result.Should().BeEquivalentTo(expectedResult);
        }

        [Fact, IsUnit]
        public async Task When_RepositoryFailsAddingSubscriberAfterSuccessfulReaderOperation_Then_OperationFails()
        {
            _repositoryMock.Setup(r => r.GetSubscriberAsync(It.Is<SubscriberId>(id => id.Equals(new SubscriberId("event", "subscriber")))))
                .ReturnsAsync(new EntityNotFoundError("subscriber", "key"));
            _requestsGeneratorMock.Setup(r => r.DefineChangesAsync(It.IsAny<SubscriberEntity>(), It.IsAny<SubscriberEntity>()))
               .ReturnsAsync(new[] { new UpdateReader() });
            _directorServiceProxyMock.Setup(r => r.CallDirectorServiceAsync(It.IsAny<ReaderChangeBase>()))
                .ReturnsAsync(new SubscriberConfiguration());
            _repositoryMock.Setup(r => r.AddSubscriberAsync(It.IsAny<SubscriberEntity>()))
                .ReturnsAsync(new BusinessError("test error"));

            var result = await _handler.Handle(_testRequest, CancellationToken.None);

            var expectedResult = new OperationResult<SubscriberDto>(new BusinessError("test error"));
            result.Should().BeEquivalentTo(expectedResult);
        }
    }
}