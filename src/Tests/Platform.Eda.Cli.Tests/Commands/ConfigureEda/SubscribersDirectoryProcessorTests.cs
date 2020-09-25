﻿using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CaptainHook.Api.Client.Models;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class SubscribersDirectoryProcessorTests
    {
        private const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly SubscribersDirectoryProcessor _subscribersDirectoryProcessor;

        public SubscribersDirectoryProcessorTests()
        {
            _subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(new MockFileSystem(GetOneSampleInputFile(), MockCurrentDirectory));
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

        [Theory, IsUnit]
        [InlineData(MockCurrentDirectory)]
        public void ProcessDirectory_WithValidFile_ReturnsValidObject(string testFilesDirectoryPath)
        {
            // Act
            var result = _subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    new PutSubscriberFile
                    {
                        Request = new PutSubscriberRequest
                        {
                            SubscriberName = "test-sub",
                            EventName = "test-event",
                        }
                    }
                });

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Theory, IsUnit]
        [InlineData(MockCurrentDirectory)]
        public void ProcessDirectory_WithMultipleValidFiles_ReturnsValidObject(string testFilesDirectoryPath)
        {
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(new MockFileSystem(GetMultipleMockInputFiles(), MockCurrentDirectory));
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    new PutSubscriberFile
                    {
                        Request = new PutSubscriberRequest
                        {
                            SubscriberName = "test-sub",
                            EventName = "test-event",
                        }
                    },
                    new PutSubscriberFile
                    {
                        Request = new PutSubscriberRequest
                        {
                            SubscriberName = "test-sub2",
                            EventName = "test-event2",
                        }
                    }
                });

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Theory, IsUnit]
        [InlineData(MockCurrentDirectory)]
        public void ProcessDirectory_EmptyDirectory_ReturnsValidObject(string testFilesDirectoryPath)
        {
            // Arrange
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(
                new MockFileSystem(new Dictionary<string, MockFileData>(), MockCurrentDirectory));
            
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(new List<PutSubscriberFile>());

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Fact, IsUnit]
        public void ProcessDirectory_WithInvalidJsonFile_ReturnsError()
        {
            var subscribersDirectoryProcessor = new SubscribersDirectoryProcessor(new MockFileSystem(GetInvalidJsonMockInputFile(), MockCurrentDirectory));
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);

            // Assert
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CliExecutionError>();
        }
        
        private static Dictionary<string, MockFileData> GetOneSampleInputFile()
        {
            Dictionary<string, MockFileData> mockFiles = new Dictionary<string, MockFileData>();
            mockFiles.Add("sample1.json", new MockFileData(@"
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
"));
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