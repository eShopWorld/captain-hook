using System;
using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using CaptainHook.Cli.Tests;
using Eshopworld.Tests.Core;
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
        internal const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly ConfigureEdaCommand _configureEdaCommand;
        private readonly Mock<ICaptainHookClient> _mockCaptainHookClient;

        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockCaptainHookClient = new Mock<ICaptainHookClient>();
            _configureEdaCommand = new ConfigureEdaCommand(new MockFileSystem(GetMockFiles1(), MockCurrentDirectory), _mockCaptainHookClient.Object);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenSingleFileRequestAccepted_Returns0()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = SubscribersDirectoryProcessorTests.MockCurrentDirectory;
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
            Output.SplitLines().Should()
                .Contain(
                    $@"Reading files from folder: '{SubscribersDirectoryProcessorTests.MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("File 'sample1.json' has been found")
                .And.Contain("Starting to run configuration against Captain Hook API")
                .And.Contain("File 'sample1.json' has been processed successfully")
                .And.Contain("Processing finished");

            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenMultipleSubdirectoriesRequestAccepted_Returns0()
        {
            // Arrange
            var configureEdaCommand = new ConfigureEdaCommand(new MockFileSystem(GetMockFiles2(), MockCurrentDirectory), _mockCaptainHookClient.Object);
            configureEdaCommand.InputFolderPath = MockCurrentDirectory;
            configureEdaCommand.NoDryRun = true;
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
            var result = await configureEdaCommand.OnExecuteAsync(Application, Console);

            // Assert
            Output.SplitLines().Should()
                .Contain(
                    $@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("File 'sample1.json' has been found")
                .And.Contain(@"File 'subdir\sample2.json' has been found")
                .And.Contain("Starting to run configuration against Captain Hook API")
                .And.Contain("File 'sample1.json' has been processed successfully")
                .And.Contain(@"File 'subdir\sample2.json' has been processed successfully")
                .And.Contain("Processing finished");

            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenRequestNotAccepted_Returns2()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = SubscribersDirectoryProcessorTests.MockCurrentDirectory;
            _configureEdaCommand.NoDryRun = true;
            var response = new HttpOperationResponse<object>
            {
                Response = new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent("Request rejected.")
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
            Output.SplitLines().Should()
                .Contain(
                    $@"Reading files from folder: '{SubscribersDirectoryProcessorTests.MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("File 'sample1.json' has been found")
                .And.Contain("Starting to run configuration against Captain Hook API")
                .And.NotContain("File 'sample1.json' has been processed successfully")
                .And.Contain("Error when processing 'sample1.json':")
                .And.Contain("Status code: 409");
            result.Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenInputDirectoryPathNull_ThrowsException()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = null;

            // Act - Assert;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configureEdaCommand.OnExecuteAsync(Application, Console));
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenNoDryRunFalse_ApiIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.InputFolderPath = SubscribersDirectoryProcessorTests.MockCurrentDirectory;
            _configureEdaCommand.NoDryRun = false;

            // Act;
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console);

            // Assert
            _mockCaptainHookClient.Verify(client =>
            client.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()), Times.Never);

            Output.SplitLines().Should()
                .Contain(
                    $@"Reading files from folder: '{SubscribersDirectoryProcessorTests.MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("File 'sample1.json' has been found")
                .And.NotContain("Starting to run configuration against Captain Hook API")
                .And.Contain("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch")
                .And.Contain("Processing finished");
        }

        public static Dictionary<string, MockFileData> GetMockFiles1()
        {
            var mockFiles = new Dictionary<string, MockFileData>
            {
                {
                    "sample1.json", new MockFileData(@"
{
  ""subscriberName"": ""test-sub"",
  ""eventName"": ""test-event"",
  ""subscriber"": {
    ""webhooks"": {
      ""endpoints"": [
        {
          ""uri"": ""https://blah.blah/testing"",
          ""authentication"": {
            ""type"": ""Basic"",
            ""username"": ""test"",
            ""passwordKeyName"": ""AzureSubscriptionId""
          },
          ""httpVerb"": ""post""
        }
      ]
    }
  }
}
")
                }
            };
            return mockFiles;
        }

        public static Dictionary<string, MockFileData> GetMockFiles2()
        {
            var mockFiles = new Dictionary<string, MockFileData>
            {
                {
                    "sample1.json", new MockFileData(@"
{
  ""subscriberName"": ""test-sub"",
  ""eventName"": ""test-event"",
  ""subscriber"": {
    ""webhooks"": {
      ""endpoints"": [
        {
          ""uri"": ""https://blah.blah/testing"",
          ""authentication"": {
            ""type"": ""Basic"",
            ""username"": ""test"",
            ""passwordKeyName"": ""AzureSubscriptionId""
          },
          ""httpVerb"": ""post""
        }
      ]
    }
  }
}
")
                },
                {
                    "subdir/sample2.json", new MockFileData(@"
{
  ""subscriberName"": ""test-sub2"",
  ""eventName"": ""test-event2"",
  ""subscriber"": {
    ""webhooks"": {
      ""endpoints"": [
        {
          ""uri"": ""https://blah.blah/testing2"",
          ""authentication"": {
            ""type"": ""Basic"",
            ""username"": ""test2"",
            ""passwordKeyName"": ""AzureSubscriptionId""
          },
          ""httpVerb"": ""post""
        }
      ]
    }
  }
}
")
                }
            };
            return mockFiles;
        }
    }
}