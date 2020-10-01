using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CaptainHook.Api.Client.Models;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class SubscribersDirectoryProcessorTests
    {
        private const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly SubscribersDirectoryProcessor _subscribersDirectoryProcessor;
        private readonly Mock<ISubscriberFileParser> _mockSubscriberFileParser;

        public SubscribersDirectoryProcessorTests()
        {
            var mockFs = new MockFileSystem(GetOneSampleInputFile(), MockCurrentDirectory);
            _mockSubscriberFileParser = new Mock<ISubscriberFileParser>();
            _subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(mockFs, _mockSubscriberFileParser.Object);
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
        public void ProcessDirectory_WithValidFile_ReturnsValidObject()
        {
            // Arrange
            var parsedSubFile = new PutSubscriberFile
            {
                Request = new PutSubscriberRequest
                {
                    SubscriberName = "test-sub",
                    EventName = "test-event"
                }
            };

            _mockSubscriberFileParser.Setup(parser => parser.ParseFile(It.IsAny<string>()))
                .Returns(parsedSubFile);

            // Act
            var result = _subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);
            
            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    parsedSubFile
                });

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Fact, IsUnit]
        public void ProcessDirectory_WithMultipleValidFiles_FileParserIsCalledForAll()
        {
            var mockFileSystem = new MockFileSystem(GetMultipleMockInputFiles(), MockCurrentDirectory);
            var mockSubscriberFileParser = new Mock<ISubscriberFileParser>();
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(mockFileSystem, mockSubscriberFileParser.Object);

            mockSubscriberFileParser.Setup(parser => parser.ParseFile(It.IsAny<string>()))
                .Returns(new PutSubscriberFile());

            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);

            // Assert
            mockSubscriberFileParser.Verify(parser => parser.ParseFile(Path.Combine(MockCurrentDirectory, "sample1.json")), Times.Once);
            mockSubscriberFileParser.Verify(parser => parser.ParseFile(Path.Combine(MockCurrentDirectory, "subdir\\sample2.json")), Times.Once);
            result.IsError.Should().BeFalse();
        }

        [Fact, IsUnit]
        public void ProcessDirectory_EmptyDirectory_ReturnsValidObject()
        {
            // Arrange
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(
                new MockFileSystem(new Dictionary<string, MockFileData>(), MockCurrentDirectory), Mock.Of<ISubscriberFileParser>());

            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(new CliExecutionError("No files have been found in 'Z:\\Sample\\'"));

            result.Should().BeEquivalentTo(expected);

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
