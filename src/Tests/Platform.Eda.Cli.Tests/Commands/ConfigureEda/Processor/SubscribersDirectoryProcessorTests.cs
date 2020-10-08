using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscribersDirectoryProcessorTests
    {

        private const string MockDirectoryPath = @"Z:\Sample\";
        private readonly SubscribersDirectoryProcessor _subscribersDirectoryProcessor;

        public SubscribersDirectoryProcessorTests()
        {
            var mockFs = new MockFileSystem(GetOneSampleInputFile(), MockDirectoryPath);
            _subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(mockFs);
        }

        [Theory, IsUnit]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentException))]
        public void ProcessDirectory_WhenPathEmptyOrNull_ThrowsException(string testFilesDirectoryPath, Type exceptionType)
        {
            Assert.Throws(exceptionType, () =>
                _subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath));
        }

        [Theory, IsUnit]
        [InlineData("a-nonexistent-path")]
        public void ProcessDirectory_WithNonExistentPath_ReturnsError(string testFilesDirectoryPath)
        {
            var result = _subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath);
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CliExecutionError>();
        }

        [Fact, IsUnit]
        public void ProcessDirectory_EmptyDirectory_ReturnsError()
        {
            // Arrange
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(new MockFileSystem(new Dictionary<string, MockFileData>(), MockDirectoryPath));

            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(new CliExecutionError("No files have been found in 'Z:\\Sample\\'"));
            result.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void ProcessDirectory_WithSingleFile_ReturnsValidPaths()
        {
            // Act
            var result = _subscribersDirectoryProcessor.ProcessDirectory(MockDirectoryPath);

            // Assert
            var expected = new List<string>
            {
                Path.Combine(MockDirectoryPath, "sample1.json")
            };

            result.Should().BeEquivalentTo(new OperationResult<IEnumerable<string>>(expected));

        }

        [Fact, IsUnit]
        public void ProcessDirectory_WithMultipleNestedFiles_ReturnsAllFiles()
        {
            var mockFileSystem = new MockFileSystem(GetMultipleMockInputFiles(), MockDirectoryPath);
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(mockFileSystem);

            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockDirectoryPath);

            // Assert
            var expectedList = new List<string>
            {
                Path.Combine(MockDirectoryPath, "sample1.json"),
                Path.Combine(MockDirectoryPath, "subdir\\sample2.json")
            };
            result.Should().BeEquivalentTo(new OperationResult<IEnumerable<string>>(expectedList));
        }

        private static Dictionary<string, MockFileData> GetOneSampleInputFile()
        {
            return new Dictionary<string, MockFileData>
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
    }
}
