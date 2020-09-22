using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using CaptainHook.Cli.Tests;
using FluentAssertions;
using Microsoft.Rest;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Xunit;
using Xunit.Abstractions;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ConfigureEdaCommandTests : CliTestBase
    {
        private readonly ConfigureEdaCommand _configureEdaCommand;
        private readonly Mock<ICaptainHookClient> _mockCaptainHookClient;

        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockCaptainHookClient = new Mock<ICaptainHookClient>();
            _configureEdaCommand = new ConfigureEdaCommand(SubscribersDirectoryProcessorTests.GetMockFileSystem(), _mockCaptainHookClient.Object);
        }

        [Fact]
        public async Task OnExecuteAsync_WhenRequestAccepted_Returns0()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = "TestFiles";
            _configureEdaCommand.NoDryRun = true;
            var response = new HttpOperationResponse<object>
            {
                Response = new HttpResponseMessage(HttpStatusCode.Accepted)
                {
                    Content = new StringContent("Request accepted.")
                }
            };

            _mockCaptainHookClient.Setup(client =>
                client.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    null,
                    CancellationToken.None)).Returns(Task.FromResult(response));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console);

            // Assert
            result.Should().Be(0);
        }

        [Fact]
        public async Task OnExecuteAsync_WhenRequestNotAccepted_Returns2()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = "TestFiles";
            _configureEdaCommand.NoDryRun = true;
            var response = new HttpOperationResponse<object>
            {
                Response = new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent("Request rejected.")
                }
            };

            // Act
            _mockCaptainHookClient.Setup(client =>
                client.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    null,
                    CancellationToken.None)).Returns(Task.FromResult(response));

            // Assert
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console);
            result.Should().Be(2);
        }

        [Fact]
        public async Task OnExecuteAsync_WhenInputDirectoryPathNull_Returns1()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = null;

            // Act - Assert;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configureEdaCommand.OnExecuteAsync(Application, Console));
        }


        [Fact]
        public async Task OnExecuteAsync_WhenInputNoDryRunFalse_ApiIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = "TestFiles";
            _configureEdaCommand.NoDryRun = false;

            // Act - Assert;
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console); 
            _mockCaptainHookClient.Verify(client=> 
                client.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(), 
                    It.IsAny<string>(), 
                    It.IsAny<CaptainHookContractSubscriberDto>(), 
                    It.IsAny<Dictionary<string, List<string>>>(), 
                    It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}