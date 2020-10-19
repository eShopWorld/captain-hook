using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Rest;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ApiConsumerTests
    {
        private readonly Mock<ICaptainHookClient> _apiClientMock = new Mock<ICaptainHookClient>();

        private readonly ApiConsumer _apiConsumer;

        private readonly HttpOperationResponse<object> _positiveResponse = new HttpOperationResponse<object>
        {
            Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Accepted
            }
        };

        private readonly HttpOperationResponse<object> _negativeResponse = new HttpOperationResponse<object>
        {
            Response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.BadRequest,
                Content = new StringContent("test-content")
            }
        };

        private readonly FileInfoBase _fileInfo = Mock.Of<FileInfoBase>();

        public ApiConsumerTests()
        {
            _apiConsumer = new ApiConsumer(_apiClientMock.Object, new[] { TimeSpan.FromMilliseconds(50.0) });
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_OneFileToProcess_SingleResultSuccessful()
        {
            // Arrange
            _apiClientMock
                .Setup(s => s.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_positiveResponse);
            var file = new PutSubscriberFile
            {
                File = _fileInfo,
                Request = new PutSubscriberRequest()
            };

            // Act
            var result = await _apiConsumer.CallApiAsync(file);

            // Assert
            var expected = new ApiOperationResult { File = _fileInfo, Request = new PutSubscriberRequest(), Response = _positiveResponse };

            result.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_TwoFileToProcess_TwoResultSuccessful()
        {
            // Arrange
            _apiClientMock
                .Setup(s => s.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_positiveResponse);
            var files = new[]
            {
                new PutSubscriberFile
                {
                    File = _fileInfo,
                    Request = new PutSubscriberRequest()
                },
                new PutSubscriberFile
                {
                    File = _fileInfo,
                    Request = new PutSubscriberRequest()
                }
            };

            // Act
            var results = new List<ApiOperationResult>();
            foreach (var file in files)
            {
                results.Add(await _apiConsumer.CallApiAsync(file));
            }

            // Assert
            var expected = new[]
            {
                new ApiOperationResult { File = _fileInfo, Request = new PutSubscriberRequest(), Response = _positiveResponse },
                new ApiOperationResult { File = _fileInfo, Request = new PutSubscriberRequest(), Response = _positiveResponse },
            };

            results.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_TwoFileToProcessFirstOneFails_ResultsInSuccessAndFailure()
        {
            // Arrange
            _apiClientMock.SetupSequence(s => s.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_positiveResponse)
                .ReturnsAsync(_negativeResponse) //1st call for 2nd file
                .ReturnsAsync(_negativeResponse); //2nd call for 2nd file
            var files = new[]
            {
                new PutSubscriberFile
                {
                    File = _fileInfo,
                    Request = new PutSubscriberRequest()
                },
                new PutSubscriberFile
                {
                    File = _fileInfo,
                    Request = new PutSubscriberRequest()
                }
            };

            // Act
            var results = new List<ApiOperationResult>();
            foreach (var file in files)
            {
                results.Add(await _apiConsumer.CallApiAsync(file));
            }

            // Assert
            var expected = new[]
            {
                new ApiOperationResult { File = _fileInfo, Request = new PutSubscriberRequest(), Response = _positiveResponse },
                new ApiOperationResult { File = _fileInfo, Request = new PutSubscriberRequest(), Response = new CliExecutionError("Status code: 400\r\nReason: Bad Request\r\nResponse: test-content") },
            };

            results.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_OneFileToProcessExpectedToFail_TwoCallsAreMade()
        {
            // Arrange
            _apiClientMock.SetupSequence(s => s.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_negativeResponse) //1st call for 2nd file
                .ReturnsAsync(_negativeResponse); //2nd call for 2nd file
            
            var file = new PutSubscriberFile
            {
                File = _fileInfo,
                Request = new PutSubscriberRequest()
            };

            // Act
            var _ = await _apiConsumer.CallApiAsync(file);

            // Assert
            _apiClientMock.Verify(s => s.PutSuscriberWithHttpMessagesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CaptainHookContractSubscriberDto>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }
    }
}