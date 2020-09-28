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

namespace Platform.Eda.Cli.Tests.ConfigureEda
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

        private readonly string _fileName = "a-file-name";

        public ApiConsumerTests()
        {
            _apiConsumer = new ApiConsumer(_apiClientMock.Object, new[] { TimeSpan.FromMilliseconds(50.0) });
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_OneFileToProcess_SingleResultSuccessful()
        {
            // Arrange
            var request = new PutSubscriberRequest();

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
                new PutSubscriberFile(_fileName, request)
            };

            // Act
            var results = await GetAll(_apiConsumer.CallApiAsync(files));

            // Assert
            var expected = new[]
            {
                new ApiOperationResult(_fileName, request, _positiveResponse)
            };

            results.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_TwoFileToProcess_TwoResultSuccessful()
        {
            // Arrange
            var request1 = new PutSubscriberRequest();
            var request2 = new PutSubscriberRequest();

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
                new PutSubscriberFile(_fileName, request1),
                new PutSubscriberFile(_fileName, request2)
            };

            // Act
            var results = await GetAll(_apiConsumer.CallApiAsync(files));

            // Assert
            var expected = new[]
            {
                new ApiOperationResult(_fileName, request1, _positiveResponse),
                new ApiOperationResult(_fileName, request2, _positiveResponse),
            };

            results.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_TwoFileToProcessFirstOneFails_ResultsInSuccessAndFailure()
        {
            // Arrange
            var request1 = new PutSubscriberRequest();
            var request2 = new PutSubscriberRequest();

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
                new PutSubscriberFile(_fileName, request1),
                new PutSubscriberFile(_fileName, request2)
            };

            // Act
            var results = await GetAll(_apiConsumer.CallApiAsync(files));

            // Assert
            var expected = new[]
            {
               new ApiOperationResult(_fileName, request1, _positiveResponse),
               new ApiOperationResult(_fileName, request2, new CliExecutionError("Status code: 400\r\nReason: Bad Request\r\nResponse: test-content")),
            };

            results.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public async Task CallApiAsync_OneFileToProcessExpectedToFail_TwoCallsAreMade()
        {
            // Arrange
            var request = new PutSubscriberRequest();

            _apiClientMock.SetupSequence(s => s.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_negativeResponse) //1st call for 2nd file
                .ReturnsAsync(_negativeResponse); //2nd call for 2nd file
            var files = new[]
            {
                new PutSubscriberFile(_fileName, request)
            };

            // Act
            var _ = await GetAll(_apiConsumer.CallApiAsync(files));

            // Assert
            _apiClientMock.Verify(s => s.PutSuscriberWithHttpMessagesAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CaptainHookContractSubscriberDto>(),
                It.IsAny<Dictionary<string, List<string>>>(),
                It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        private static async Task<IEnumerable<T>> GetAll<T>(IAsyncEnumerable<T> enumerable)
        {
            var list = new List<T>();
            await foreach (var item in enumerable)
            {
                list.Add(item);
            }

            return list;
        }
    }
}