using System;
using System.Collections.Generic;
using System.Threading;
using CaptainHook.Api.Controllers;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using System.Threading.Tasks;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
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
        private EventsController _sut;

        public EventsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _sut =  new EventsController(_mediatorMock.Object);
        }

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

        public class SomeOtherError : ErrorBase
        {
            public SomeOtherError() :
                base("Some other error")
            {
            }
        }

    }
}
