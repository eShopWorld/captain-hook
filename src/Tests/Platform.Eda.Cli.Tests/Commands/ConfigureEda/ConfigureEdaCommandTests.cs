using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
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
    public class ConfigureEdaCommandTests
    {
        internal const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly ConfigureEdaCommand _configureEdaCommand;

        private readonly Mock<IApiConsumer> _apiConsumer;
        private readonly Mock<IConsoleSubscriberWriter> _mockConsoleSubscriberWriter;
        private readonly Mock<ISubscribersDirectoryProcessor> _mockSubscribersDirectoryProcessor;

        public ConfigureEdaCommandTests()
        {
            _mockConsoleSubscriberWriter = new Mock<IConsoleSubscriberWriter>();
            _apiConsumer = new Mock<IApiConsumer>();
            _mockSubscribersDirectoryProcessor = new Mock<ISubscribersDirectoryProcessor>();

            _configureEdaCommand = new ConfigureEdaCommand(_mockSubscribersDirectoryProcessor.Object, env=> _apiConsumer.Object)
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
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableResponse(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }


        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenMultipleSubdirectoriesRequestAccepted_Returns0()
        {
            // Arrange
            var files = GetMultipleMockInputFiles();
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableResponse(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenEmptyDirectory_Returns0()
        {
            // Arrange
            var files = new PutSubscriberFile[0];
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableResponse(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenProcessDirectoryError_Returns1()
        {
            // Arrange
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() =>
                    new OperationResult<IEnumerable<PutSubscriberFile>>(new CliExecutionError("Error text")));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()), Times.Never);
            _mockConsoleSubscriberWriter.Verify(writer => writer.WriteError("Error text"));
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
            await Assert.ThrowsAsync<ArgumentNullException>(() => _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object));
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
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert;
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenNoDryRunFalse_ApiIsNotCalled()
        {
            // Arrange
            _configureEdaCommand.NoDryRun = false;
            var files = new PutSubscriberFile[0];
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter.Object);

            // Assert
            _mockSubscribersDirectoryProcessor.Verify(proc => proc.ProcessDirectory(MockCurrentDirectory), Times.Once);
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()), Times.Never);
            VerifySubscriberWriterCallsForFiles(files);
            result.Should().Be(0);
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
            _mockConsoleSubscriberWriter.Verify(writer => writer.OutputSubscribers(files, MockCurrentDirectory), Times.Once);
            _mockConsoleSubscriberWriter.Verify(writer => writer.WriteSuccess("box",
                    $@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment"), Times.Once);
        }

#pragma warning disable 1998
        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableResponse(IEnumerable<PutSubscriberFile> files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo($"{putSubscriberFile}"),
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
                    File = new FileInfo($"{putSubscriberFile}"),
                    Response = new CliExecutionError("Exception text", new Failure("0", "Failure message"))
                };
            }
        }
#pragma warning restore 1998
    }
}