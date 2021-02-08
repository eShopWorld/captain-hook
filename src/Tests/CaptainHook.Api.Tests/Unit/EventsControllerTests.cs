using System;
using System.Threading;
using CaptainHook.Api.Controllers;
using Eshopworld.Tests.Core;
using Moq;
using System.Threading.Tasks;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CaptainHook.Api.Tests.Unit
{
    public class EventsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly EventsController _sut;

        public EventsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _sut =  new EventsController(_mediatorMock.Object);
        }

        #region PutWebhook

        [Fact, IsUnit]
        public async Task PutWebhook_WhenSuccessful_ReturnsCreated()
        {
            // Arrange 
            var endpointDto = new EndpointDto();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(endpointDto);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<CreatedResult>()
                .Which.Value.Should().Be(endpointDto);
        }

        [Fact, IsUnit]
        public async Task PutWebhook_OnValidationError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new ValidationError("an error");

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutWebhook_OnDirectorServiceIsBusyError_ReturnsConflict()
        {
            // Arrange 
            var mediatorResult = new DirectorServiceIsBusyError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutWebhook_OnReaderCreateError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderCreateError("subscriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutWebhook_OnCannotSaveEntityError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new CannotSaveEntityError("subscriber", new Exception());

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutWebhook_OnUnknownError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new SomeOtherError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutWebhook("event", "subscriber", "*", new EndpointDto());

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }

        #endregion

        #region DeleteWebhook

        [Fact, IsUnit]
        public async Task DeleteWebhook_WhenSuccessful_ReturnsOk()
        {
            // Arrange 
            var mediatorResult = new SubscriberDto();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnValidationError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new ValidationError("an error");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnCannotRemoveLastEndpointFromSubscriberError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new CannotRemoveLastEndpointFromSubscriberError(new SubscriberEntity("subscriber"));

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnEndpointNotFoundInSubscriberError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new EndpointNotFoundInSubscriberError("selector");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnEntityNotFoundError_ReturnsNotFound()
        {
            // Arrange 
            var mediatorResult = new EntityNotFoundError("type", "key");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnDirectorServiceIsBusyError_ReturnsConflict()
        {
            // Arrange 
            var mediatorResult = new DirectorServiceIsBusyError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnReaderDeleteError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderDeleteError("subscriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnReaderDoesNotExistError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderDoesNotExistError("subscriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnCannotUpdateEntityError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new CannotUpdateEntityError("subscriber", new Exception());

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }

        [Fact, IsUnit]
        public async Task DeleteWebhook_OnUnknownError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new SomeOtherError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteWebhookRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteWebhook("event", "subscriber", "*");

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }
        #endregion

        #region PutSubscriber

        [Fact, IsUnit]
        public async Task PutSubscriber_WhenSuccessfullyCreated_ReturnsCreated()
        {
            // Arrange 
            var subscriber = new SubscriberDto();
            var mediatorResult = new UpsertResult<SubscriberDto>(subscriber, UpsertType.Created);

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<CreatedResult>()
                .Which.Value.Should().Be(subscriber);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_WhenSuccessfullyUpdated_ReturnsAccepted()
        {
            // Arrange 
            var subscriber = new SubscriberDto();
            var mediatorResult = new UpsertResult<SubscriberDto>(subscriber, UpsertType.Updated);

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<AcceptedResult>()
                .Which.Value.Should().Be(subscriber);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_OnValidationError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new ValidationError("error");

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_OnDirectorServiceIsBusyError_ReturnsConflict()
        {
            // Arrange 
            var mediatorResult = new DirectorServiceIsBusyError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_OnReaderCreateError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderCreateError("subscriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_OnCannotSaveEntityError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new CannotSaveEntityError("subscriber", new Exception());

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task PutSubscriber_OnUnknownError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new SomeOtherError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<UpsertSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.PutSubscriber("event", "subscriber", new SubscriberDto());

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }

        #endregion

        #region DeleteSubscriber

        [Fact, IsUnit]
        public async Task DeleteSubscriber_WhenSuccessful_ReturnsOk()
        {
            // Arrange 
            var mediatorResult = new SubscriberDto();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnValidationError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new ValidationError("error");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnCannotRemoveLastEndpointFromSubscriberError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new CannotRemoveLastEndpointFromSubscriberError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnEndpointNotFoundInSubscriberError_ReturnsBadRequest()
        {
            // Arrange 
            var mediatorResult = new EndpointNotFoundInSubscriberError("selector");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<BadRequestObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnEntityNotFoundError_ReturnsNotFound()
        {
            // Arrange 
            var mediatorResult = new EntityNotFoundError("type", "key");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnDirectorServiceIsBusyError_ReturnsConflict()
        {
            // Arrange 
            var mediatorResult = new DirectorServiceIsBusyError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<ConflictObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnReaderDeleteError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderDeleteError("susbcriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnReaderDoesNotExistError_ReturnsUnprocessableEntity()
        {
            // Arrange 
            var mediatorResult = new ReaderDoesNotExistError("susbcriber", "event");

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<UnprocessableEntityObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnCannotUpdateEntityError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new CannotUpdateEntityError("susbcriber", new Exception());

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }

        [Fact, IsUnit]
        public async Task DeleteSubscriber_OnUnknownError_ReturnsInternalServerError()
        {
            // Arrange 
            var mediatorResult = new SomeOtherError();

            _mediatorMock.Setup(x => x.Send(It.IsAny<DeleteSubscriberRequest>(), CancellationToken.None))
                .ReturnsAsync(mediatorResult);

            // Act
            var apiResponse = await _sut.DeleteSubscriber("event", "subscriber");

            // Assert
            apiResponse.Should().BeOfType<ObjectResult>()
                .Which.Value.Should().Be(mediatorResult);
            ((ObjectResult)apiResponse).StatusCode.Should().Be(500);
        }

        #endregion



        public class SomeOtherError : ErrorBase
        {
            public SomeOtherError() :
                base("Some other error")
            {
            }
        }

    }
}
