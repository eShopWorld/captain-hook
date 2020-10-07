/*using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Cli.Tests;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.Rest;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
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

            _configureEdaCommand = new ConfigureEdaCommand(Mock.Of<IPutSubscriberProcessChain>())
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
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].File.FullName)}' has been processed successfully");
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
                Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, putSubscriberFile.File.FullName)}' has been processed successfully");
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
            OutputShouldContainFileNames(files);
            Output.Should()
                .Contain($"Error when processing '{files[0].File.Name}'")
                .And.Contain("Exception text");
            result.Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_OnApiPartialFailure_Returns2()
        {
            // Arrange
            var files = GetMultipleMockInputFiles();
            _mockSubscribersDirectoryProcessor.Setup(proc => proc.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(files));
            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(files))
                .Returns(AsyncEnumerableMixed(files));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(files), Times.Once);

            // OutputsFailureAndSuccess
            OutputShouldContainFileNames(files);
            Output.Should().Contain($"Error when processing '{Path.GetRelativePath(MockCurrentDirectory, files[0].File.FullName)}'");
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[1].File.FullName)}' has been processed successfully");

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
            Output.Should().Contain($"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].File.FullName)}' has been processed successfully");
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
                    $"File '{Path.GetRelativePath(MockCurrentDirectory, files[0].File.FullName)}' has been processed successfully")
                .And.NotContain("Starting to run configuration against Captain Hook API")
                .And.Contain(
                    "By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");

            result.Should().Be(0);
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenErrorInProcessFile_FileIsNotPassedToApi()
        {
            // Arrange
            _configureEdaCommand.NoDryRun = true;
            _configureEdaCommand.InputFolderPath = MockCurrentDirectory;
            var errorText = "Error text";
            _mockSubscribersDirectoryProcessor.Setup(processor => processor.ProcessDirectory(MockCurrentDirectory))
                .Returns(() => new OperationResult<IEnumerable<PutSubscriberFile>>(new List<PutSubscriberFile>
                {
                    new PutSubscriberFile
                    {
                        File = new FileInfo(Path.Combine(MockCurrentDirectory, "TheGoodFile.json")), 
                        Error = null
                    },
                    new PutSubscriberFile
                    {
                        File = new FileInfo(Path.Combine(MockCurrentDirectory, "TheBadFile.json")),
                        Error = errorText
                    }
                }));

            _apiConsumer.Setup(apiConsumer => apiConsumer.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()))
                .Returns(AsyncEnumerableResponse(new PutSubscriberFile[0]));

            // Act
            var result = await _configureEdaCommand.OnExecuteAsync(_mockConsoleSubscriberWriter);

            // Assert
            _apiConsumer.Verify(apiConsumer => apiConsumer.CallApiAsync(
                It.Is<IEnumerable<PutSubscriberFile>>(files=> files.Count() == 1 
                                                              && files.Count(file => file.File.Name == "TheGoodFile.json") == 1)), Times.Once);
            Output.Should()
                .Contain("File 'TheGoodFile.json' has been found")
                .And.Contain($"File 'TheBadFile.json' has been found, but will be skipped due to error: {errorText}.");
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
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "sample1.json")),
                    Request = new PutSubscriberRequest
                    {
                        EventName = "test-event1",
                        SubscriberName = "test-event1-sub1"
                    }
                }
            };
        }

        private static PutSubscriberFile[] GetMultipleMockInputFiles()
        {
            return new[]
            {
                new PutSubscriberFile
                {
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "sample1.json")),
                    Request = new PutSubscriberRequest
                    {
                        EventName = "test-event1",
                        SubscriberName = "test-event1-sub1"
                    }
                },
                new PutSubscriberFile
                {
                    File = new FileInfo(Path.Combine(MockCurrentDirectory, "subdir/sample2.json")),
                    Request = new PutSubscriberRequest
                    {
                        EventName = "test-event2",
                        SubscriberName = "test-event2-sub1"
                    }
                }
            };
        }

        private void OutputShouldContainFileNames(PutSubscriberFile[] files)
        {
            Output.Should().Contain($@"Reading files from folder: '{MockCurrentDirectory}' to be run against CI environment");
            foreach (var putSubscriberFile in files)
            {
                var fileRelativePath = Path.GetRelativePath(MockCurrentDirectory, putSubscriberFile.File.FullName);
                Output.Should().Contain($"File '{fileRelativePath}' has been found");
            }
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableResponse(PutSubscriberFile[] files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo(putSubscriberFile.File.FullName),
                    Request = putSubscriberFile.Request,
                    Response = new OperationResult<HttpOperationResponse>(new HttpOperationResponse
                    {
                        Response = new HttpResponseMessage(HttpStatusCode.Accepted)
                    })
                };
            }

            await Task.CompletedTask;
        }

        private static async IAsyncEnumerable<ApiOperationResult> AsyncEnumerableException(PutSubscriberFile[] files)
        {
            foreach (var putSubscriberFile in files)
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo(putSubscriberFile.File.FullName),
                    Request = putSubscriberFile.Request,
                    Response = new CliExecutionError("Exception text", new Failure("0", "Failure message"))
                };
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
                    yield return new ApiOperationResult
                    {
                        File = new FileInfo(putSubscriberFile.File.FullName),
                        Request = putSubscriberFile.Request,
                        Response = new CliExecutionError("Exception text", new Failure("0", "Failure message"))
                    };
                }
                else
                {
                    yield return new ApiOperationResult
                    {
                        File = new FileInfo(putSubscriberFile.File.FullName),
                        Request = putSubscriberFile.Request,
                        Response = new OperationResult<HttpOperationResponse>(new HttpOperationResponse
                        {
                            Response = new HttpResponseMessage(HttpStatusCode.Accepted)
                        })
                    };
                }
            }

            await Task.CompletedTask;
        }
    }
}*/