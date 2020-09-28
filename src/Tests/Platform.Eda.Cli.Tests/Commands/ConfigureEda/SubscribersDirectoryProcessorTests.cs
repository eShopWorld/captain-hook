using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using CaptainHook.Api.Client.Models;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class SubscribersDirectoryProcessorTests
    {
        private const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly SubscribersDirectoryParser _subscribersDirectoryParser;
        private readonly IConsoleSubscriberWriter _writer;

        public SubscribersDirectoryProcessorTests()
        {
            _writer = Mock.Of<IConsoleSubscriberWriter>();
            _subscribersDirectoryParser = new SubscribersDirectoryParser(new MockFileSystem(GetOneSampleInputFile(), MockCurrentDirectory), _writer);
        }

        [Theory, IsUnit]
        [InlineData(null, typeof(ArgumentNullException))]
        [InlineData("", typeof(ArgumentException))]
        public void ProcessDirectory_WhenPathEmptyOrNull_ThrowsException(string testFilesDirectoryPath, Type exceptionType)
        {
            Assert.Throws(exceptionType, () =>
                _subscribersDirectoryParser.ProcessDirectory(testFilesDirectoryPath));
        }

        [Theory, IsUnit]
        [InlineData("a-nonexistent-path")]
        public void ProcessDirectory_WithNonExistentPath_ReturnsError(string testFilesDirectoryPath)
        {
            var result = _subscribersDirectoryParser.ProcessDirectory(testFilesDirectoryPath);
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CliExecutionError>();
        }

        [Theory, IsUnit]
        [InlineData(MockCurrentDirectory)]
        public void ProcessDirectory_WithValidFile_ReturnsValidObject(string testFilesDirectoryPath)
        {
            // Act
            var result = _subscribersDirectoryParser.ProcessDirectory(testFilesDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    new PutSubscriberFile("a-file-name", new PutSubscriberRequest
                    {
                        SubscriberName = "test-sub",
                        EventName = "test-event",
                    })
                });

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Theory, IsUnit]
        [InlineData(MockCurrentDirectory)]
        public void ProcessDirectory_WithMultipleValidFiles_ReturnsValidObject(string testFilesDirectoryPath)
        {
            var subscribersDirectoryProcessor = new SubscribersDirectoryParser(new MockFileSystem(GetMultipleMockInputFiles(), MockCurrentDirectory), _writer);
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(testFilesDirectoryPath);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    new PutSubscriberFile("filename1", new PutSubscriberRequest
                    {
                        SubscriberName = "test-sub",
                        EventName = "test-event",
                    }),
                    new PutSubscriberFile("filename2", new PutSubscriberRequest
                    {
                        SubscriberName = "test-sub2",
                        EventName = "test-event2",
                    })
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
            var subscribersDirectoryProcessor = new SubscribersDirectoryParser(
                new MockFileSystem(new Dictionary<string, MockFileData>(), MockCurrentDirectory), _writer);
            
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
            var subscribersDirectoryProcessor = new SubscribersDirectoryParser(new MockFileSystem(GetInvalidJsonMockInputFile(), MockCurrentDirectory), _writer);
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);

            // Assert
            result.IsError.Should().BeTrue();
            result.Error.Should().BeOfType<CliExecutionError>();
        }


        [Fact, IsUnit]
        public void ProcessDirectory_JsonFileWithCallbacksDlqHooks_ReturnsValidObject()
        {
            var subscribersDirectoryProcessor = new SubscribersDirectoryParser(new MockFileSystem(GetSampleInputFileWithDlq(), MockCurrentDirectory), _writer);
            // Act
            var result = subscribersDirectoryProcessor.ProcessDirectory(MockCurrentDirectory);

            // Assert
            var expected = new OperationResult<IEnumerable<PutSubscriberFile>>(
                new[]
                {
                    new PutSubscriberFile("filename", new PutSubscriberRequest
                    {
                        SubscriberName = "test-sub",
                        EventName = "test-event",
                        Subscriber = new CaptainHookContractSubscriberDto()
                        {
                            Webhooks = new CaptainHookContractWebhooksDto
                            {
                                Endpoints = new List<CaptainHookContractEndpointDto>
                                {
                                    new CaptainHookContractEndpointDto("https://webhook.site/testing", "post")
                                }
                            },
                            Callbacks = new CaptainHookContractWebhooksDto
                            {
                                Endpoints = new List<CaptainHookContractEndpointDto>
                                {
                                    new CaptainHookContractEndpointDto("https://webhook.site/callbacktesting", "put")
                                }
                            },
                            DlqHooks = new CaptainHookContractWebhooksDto
                            {
                                Endpoints = new List<CaptainHookContractEndpointDto>
                                {
                                    new CaptainHookContractEndpointDto("https://webhook.site/dlqtesting", "post")
                                }
                            }
                        }
                    })
                });

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.Name == "Authentication")
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));   

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

        private static Dictionary<string, MockFileData> GetSampleInputFileWithDlq()
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
		    ""endpoints"": [{
			    ""uri"": ""https://webhook.site/testing"",
			    ""authentication"": {
				    ""type"": ""Basic"",
				    ""username"": ""test"",
				    ""passwordKeyName"": ""AzureSubscriptionId""
			    },
			    ""httpVerb"": ""post""		
		    }]
	    },
	    ""callbacks"": {
		    ""endpoints"": [{
			    ""uri"": ""https://webhook.site/callbacktesting"",
			    ""authentication"": {
				    ""type"": ""Basic"",
				    ""username"": ""test"",
				    ""passwordKeyName"": ""AzureSubscriptionId""
			    },
			    ""httpVerb"": ""put""		
		    }]
	    },
	    ""dlqHooks"": {
		    ""endpoints"": [{
			    ""uri"": ""https://webhook.site/dlqtesting"",
			    ""authentication"": {
				    ""type"": ""Basic"",
				    ""username"": ""test"",
				    ""passwordKeyName"": ""AzureSubscriptionId""
			    },
			    ""httpVerb"": ""post""		
		    }]
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
