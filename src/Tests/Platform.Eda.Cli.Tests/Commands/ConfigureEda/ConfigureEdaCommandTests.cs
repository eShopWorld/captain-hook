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

        private readonly IApiConsumer _apiConsumer;
        private readonly IConsoleSubscriberWriter _mockConsoleSubscriberWriter;

        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockCaptainHookClient = new Mock<ICaptainHookClient>();
            _mockConsoleSubscriberWriter = new ConsoleSubscriberWriter(Console);
            _apiConsumer = new ApiConsumer(_mockCaptainHookClient.Object, null);

            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(new MockFileSystem(GetSingleMockInputFile(), MockCurrentDirectory));
            _configureEdaCommand = new ConfigureEdaCommand(subscribersDirectoryProcessor, _apiConsumer);

            _configureEdaCommand.InputFolderPath = MockCurrentDirectory;
            _configureEdaCommand.NoDryRun = true;
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenSingleFileRequestAccepted_Returns0()
        {
            // Arrange
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
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);

            // Assert
            Output.SplitLines().Should()
                .Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment")
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
            var configureEdaCommand = new ConfigureEdaCommand(new SubscribersDirectoryProcessor(new MockFileSystem(GetMultipleMockInputFiles(), MockCurrentDirectory)), _apiConsumer);
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
            var result = await configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);
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
        public async Task OnExecuteAsync_WhenEmptyDirectory_Returns0()
        {
            // Arrange
            var configureEdaCommand = new ConfigureEdaCommand(new SubscribersDirectoryProcessor(new MockFileSystem(new Dictionary<string, MockFileData>(), MockCurrentDirectory)), _apiConsumer);
            configureEdaCommand.InputFolderPath = MockCurrentDirectory;
            configureEdaCommand.NoDryRun = true;

            // Act
            var result = await configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);

            // Assert
            Output.SplitLines().Should()
                .Contain(
                    $@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.")
                .And.Contain("Starting to run configuration against Captain Hook API")
                .And.Contain("Processing finished");

            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenRequestNotAccepted_Returns2()
        {
            // Arrange
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
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);

            // Assert
            Output.SplitLines().Should()
                .Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment")
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter));
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenNoDryRunFalse_ApiIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.NoDryRun = false;

            // Act;
            var result = await _configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);
            
            // Assert
            _mockCaptainHookClient.Verify(client =>
            client.PutSuscriberWithHttpMessagesAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CaptainHookContractSubscriberDto>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()), Times.Never);

            Output.SplitLines().Should()
                .Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment")
                .And.Contain("File 'sample1.json' has been found")
                .And.NotContain("Starting to run configuration against Captain Hook API")
                .And.Contain("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch")
                .And.Contain("Processing finished");
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenInvalidJson_Returns1()
        {
            // Arrange
            var configureEdaCommand = new ConfigureEdaCommand(
                new SubscribersDirectoryProcessor(new MockFileSystem(GetInvalidJsonMockInputFile(), MockCurrentDirectory)),
                _apiConsumer);
            configureEdaCommand.InputFolderPath = MockCurrentDirectory;
            configureEdaCommand.NoDryRun = false;

            // Act;
            var result = await configureEdaCommand.OnExecuteAsync(Application, Console, _mockConsoleSubscriberWriter);

            // Assert
            result.Should().Be(1);
            Output.Should()
                .Contain("Reading files from folder: 'Z:\\Sample\\' to be run against CI environment")
                .And.Contain("WARNING - Command Platform.Eda.Cli.Commands.ConfigureEda.ConfigureEdaCommand");
        }

        private static Dictionary<string, MockFileData> GetSingleMockInputFile()
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

        private static Dictionary<string, MockFileData> GetMultipleMockInputFiles()
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

        private static Dictionary<string, MockFileData> GetInvalidJsonMockInputFile()
        {
            var mockFiles = new Dictionary<string, MockFileData>
            {
                {
                    "sample3.json", new MockFileData(@"<json>File</json>")
                }
            };
            return mockFiles;
        }
    }
}