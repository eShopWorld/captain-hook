using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using CaptainHook.Cli.Tests;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Rest;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;
using Xunit.Abstractions;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ConfigureEdaCommandTests : CliTestBase
    {
        internal const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly ConfigureEdaCommand _configureEdaCommand;

        private readonly Mock<IApiConsumer> _apiConsumer;
        private readonly ConsoleSubscriberWriter _mockConsoleSubscriberWriter;
        private readonly Mock<ISubscribersDirectoryParser> _mockSubscribersDirectoryParser;
        private readonly Mock<ISubscribersProcessor> _mockSubscribersProcessor;

        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockConsoleSubscriberWriter = new ConsoleSubscriberWriter(Console);
            _apiConsumer = new Mock<IApiConsumer>();
            _mockSubscribersDirectoryParser = new Mock<ISubscribersDirectoryParser>();
            _mockSubscribersProcessor = new Mock<ISubscribersProcessor>();

            _configureEdaCommand = new ConfigureEdaCommand(_mockSubscribersDirectoryParser.Object, _mockSubscribersProcessor.Object)
            {
                InputFolderPath = MockCurrentDirectory,
                NoDryRun = true
            };
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenSingleFileRequestAccepted_Returns0()
        {
            // Arrange
            var files = GetOneSampleInputFile();
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            OutputShouldContainFileNames(files);
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].Filename)}' has been processed successfully");
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenMultipleSubdirectoriesRequestAccepted_Returns0()
        {
            // Arrange
            var files = GetMultipleMockInputFiles();
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            OutputShouldContainFileNames(files);
            foreach (var putSubscriberFile in files)
            {
                Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, putSubscriberFile.Filename)}' has been processed successfully");
            }
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenEmptyDirectory_Returns0()
        {
            // Arrange
            var files = new PutSubscriberFile[0];
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            OutputShouldContainFileNames(files);
            Output.Should()
                .Contain("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.")
                .And.NotContain("has been processed successfully");
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenProcessDirectoryError_Returns1()
        {
            // Arrange
            _mockSubscribersDirectoryParser.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(new CliExecutionError("Error text")));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            Output.Should().Contain("Error text");
            result.Should().Be(1);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenInputDirectoryPathNull_ThrowsException()
        {
            // Arrange
            _mockSubscribersDirectoryParser.Setup(proc => proc.ProcessDirectory(null))
                .Throws<ArgumentNullException>();
            _configureEdaCommand.InputFolderPath = null;

            // Act - Assert;
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter));
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_OnApiError_Returns2()
        {
            // Arrange
            var files = GetOneSampleInputFile();
            _mockSubscribersDirectoryParser.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _mockSubscribersProcessor.Setup(x => x.ConfigureEdaAsync(files, "CI"))
                .ReturnsAsync(AsyncEnumerableException(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert;
            OutputShouldContainFileNames(files);
            Output.SplitLines().Should()
                .Contain($"Error when processing '{files[0].Filename}':")
                .And.Contain("Exception text");
            result.Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_OnApiPartialFailure_Returns2()
        {
            // Arrange
            var files = GetMultipleMockInputFiles();
            _mockSubscribersDirectoryParser.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableMixed(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);

            // OutputsFailureAndSuccess
            OutputShouldContainFileNames(files);
            Output.Should().Contain($"Error when processing '{Path.GetRelativePath(MockCurrentDirectory, files[0].Filename)}'");
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[1].Filename)}' has been processed successfully");

            result.Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_ValidFile_ApiConsumerIsCalled()
        {
            // Arrange
            var files = GetOneSampleInputFile();
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);
            OutputShouldContainFileNames(files);
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].Filename)}' has been processed successfully");
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenNoDryRunFalse_ApiConsumerIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.NoDryRun = false;
            var files = GetOneSampleInputFile();
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()), Times.Never);
            OutputShouldContainFileNames(files);
            Output.Should()
                .NotContain(
                    $"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].Filename)}' has been processed successfully")
                .And.NotContain("Starting to run configuration against Captain Hook API")
                .And.Contain(
                    "By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");

            result.Should().Be(0);
        }

        private void SetupDirectoryProcessorAndApiConsumer(PutSubscriberFile[] files)
        {
            _mockSubscribersDirectoryParser.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableResponse(files));
        }

        private static PutSubscriberFile[] GetOneSampleInputFile()
        {
            return new[]
            {
                new PutSubscriberFile(Path.Combine(MockCurrentDirectory, "sample1.json"), new PutSubscriberRequest())
            };
        }

        private static PutSubscriberFile[] GetMultipleMockInputFiles()
        {
            return new[]
            {
                new PutSubscriberFile(Path.Combine(MockCurrentDirectory, "sample1.json"), new PutSubscriberRequest()),
                new PutSubscriberFile(Path.Combine(MockCurrentDirectory, "subdir/sample2.json"), new PutSubscriberRequest())
            };
        }

        private void OutputShouldContainFileNames(PutSubscriberFile[] files)
        {
            Output.Should().Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment");
            foreach (var putSubscriberFile in files)
            {
                var fileRelativePath = Path.GetRelativePath(MockCurrentDirectory, putSubscriberFile.Filename);
                Output.Should().Contain($"File '{fileRelativePath}' has been found");
            }
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableResponse(PutSubscriberFile[] files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult(
                        putSubscriberFile.Filename,
                        new PutSubscriberRequest(),
                        new OperationResult<HttpOperationResponse>(new HttpOperationResponse()));
            }

            await Task.CompletedTask;
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableException(PutSubscriberFile[] files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult(putSubscriberFile.Filename, new PutSubscriberRequest(), new CliExecutionError("Exception text", new Failure("0", "Failure message")));
            }

            await Task.CompletedTask;
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableMixed(PutSubscriberFile[] files)
        {
            for (var i = 0; i < files.Length; i++)
            {
                var putSubscriberFile = files[i];
                if (i == 0)
                {
                    yield return new ApiOperationResult(
                        putSubscriberFile.Filename,
                        new PutSubscriberRequest(),
                        new CliExecutionError("Exception text", new Failure("0", "Failure message")));
                }
                else
                {
                    yield return new ApiOperationResult(
                        putSubscriberFile.Filename,
                        new PutSubscriberRequest(),
                        new OperationResult<HttpOperationResponse>(new HttpOperationResponse()));
                }
            }

            await Task.CompletedTask;
        }
    }
}