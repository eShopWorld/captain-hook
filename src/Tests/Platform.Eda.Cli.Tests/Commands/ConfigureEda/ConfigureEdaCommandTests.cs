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
        private readonly Mock<ISubscribersDirectoryProcessor> _mockSubscribersDirectoryProcessor;

        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockConsoleSubscriberWriter = new ConsoleSubscriberWriter(Console);
            _apiConsumer = new Mock<IApiConsumer>();
            _mockSubscribersDirectoryProcessor = new Mock<ISubscribersDirectoryProcessor>();

            _configureEdaCommand = new ConfigureEdaCommand(_mockSubscribersDirectoryProcessor.Object, env => _apiConsumer.Object)
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
            VerifySubscriberWriterCallsForFiles(files);
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
            VerifySubscriberWriterCallsForFiles(files);
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
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenProcessDirectoryError_Returns1()
        {
            // Arrange
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
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
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(null))
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
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableException(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert;
            VerifySubscriberWriterCallsForFiles(files);
            Output.SplitLines().Should()
                .Contain($"Error when processing '{files[0].File.Name}':")
                .And.Contain("Exception text");
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
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenNoDryRunFalse_ApiIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.NoDryRun = false;
            var files = new PutSubscriberFile[0];
            SetupDirectoryProcessorAndApiConsumer(files);

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()), Times.Never);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }

        private void SetupDirectoryProcessorAndApiConsumer(PutSubscriberFile[] files)
        {
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableResponse(files));
        }

        private static PutSubscriberFile[] GetOneSampleInputFile()
        {
            return new[]
            {
                new PutSubscriberFile
                {
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "sample1.json"))
                }
            };
        }

        private static PutSubscriberFile[] GetMultipleMockInputFiles()
        {
            return new[]
            {
                new PutSubscriberFile
                {
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "sample1.json"))
                },
                new PutSubscriberFile
                {
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "subdir/sample2.json"))
                }
            };
        }

        private void VerifySubscriberWriterCallsForFiles(PutSubscriberFile[] files)
        {
            foreach (var putSubscriberFile in files)
            {
                
                Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, putSubscriberFile.File.FullName)}' has been found");
            }

            Output.Should().Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment");
        }

#pragma warning disable 1998 
        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableResponse(IEnumerable<PutSubscriberFile> files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo(putSubscriberFile.File.FullName),
                    Response = new OperationResult<HttpOperationResponse>(new HttpOperationResponse())
                };
            }
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableException(IEnumerable<PutSubscriberFile> files)
        {

            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo(putSubscriberFile.File.FullName),
                    Response = new CliExecutionError("Exception text", new Failure("0", "Failure message"))
                };
            }
        }
#pragma warning restore 1998
    }
}