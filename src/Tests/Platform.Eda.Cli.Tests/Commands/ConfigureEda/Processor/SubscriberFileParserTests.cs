using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using CaptainHook.Api.Client.Models;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscriberFileParserTests
    {
        private const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly SubscriberFileParser _subscriberFileParser;

        public SubscriberFileParserTests()
        {
            var mockFileSystem = new MockFileSystem(GetOneSampleInputFile(), MockCurrentDirectory);
            _subscriberFileParser = new SubscriberFileParser(mockFileSystem);
        }

        [Theory, IsUnit]
        [InlineData(null, "Value cannot be null. (Parameter 'path')")]
        [InlineData("", "Empty file name is not legal. (Parameter 'path')")]
        [InlineData("a-nonexistent-path", "Could not find file 'a-nonexistent-path'.")]
        public void ParseFile_WhenPathEmptyOrNull_ThrowsException(string testFilePath, string errorMessage)
        {
            var result = _subscriberFileParser.ParseFile(testFilePath);
            result.Should().BeEquivalentTo(new OperationResult<JObject>(new CliExecutionError(errorMessage)));
        }

        [Fact, IsUnit]
        public void ParseFile_ValidJsonFile_ReturnsValidObject()
        {
            // Act
            var result = _subscriberFileParser.ParseFile(Path.Combine(MockCurrentDirectory, "sample1.json"));

            // Assert
            result.IsError.Should().BeFalse(); 
            result.Data.Should().Contain(new KeyValuePair<string, JToken>("prop1", "val1"));
            result.Data.Should().Contain(new KeyValuePair<string, JToken>("prop2", "val2"));
        }

        [Fact, IsUnit]
        public void ParseFile_ValidJsonFileInSubDirectory_ReturnsValidObject()
        {
            var fs = new MockFileSystem(GetNestedMockInputFile(), MockCurrentDirectory);
            var subscriberFileParser = new SubscriberFileParser(fs);

            // Act
            var result = subscriberFileParser.ParseFile(fs.AllFiles.First());

            // Assert
            result.IsError.Should().BeFalse();
            result.Data.Should().Contain(new KeyValuePair<string, JToken>("prop1", "val1"));
            result.Data.Should().Contain(new KeyValuePair<string, JToken>("prop2", "val2"));
        }

        [Theory, IsUnit]
        [MemberData(nameof(InvalidJsonMockInputFiles))]
        public void ParseFile_WithInvalidJson_ReturnsObjectWithErrorInformation(IDictionary<string, MockFileData> files, string errorMessage)
        {
            // Arrange
            var subscriberFileParser = new SubscriberFileParser(new MockFileSystem(files, MockCurrentDirectory));

            // Act
            var result = subscriberFileParser.ParseFile(files.First().Key);

            // Assert
            result.IsError.Should().BeTrue();
            result.Error.Message.Should()
                .Contain(errorMessage) // expected error message
                .And.Match("* Path *, line *, position *."); // error information
        }

        [Fact, IsUnit]
        public void ParseFile_SubscriberJsonWithCallbacksDlqHooks_ReturnsValidObject()
        {
            // Arrange
            var files = GetSampleInputFileWithDlq();
            var subscriberFileParser = new SubscriberFileParser(new MockFileSystem(files, MockCurrentDirectory));

            // Act
            var result = subscriberFileParser.ParseFile("sample1.json");

            // Assert
            var expected = new PutSubscriberRequest
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
            };

            result.IsError.Should().BeFalse();
            result.Data.ToObject<PutSubscriberRequest>().Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.Name == "Authentication")
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));
        }

        public static IEnumerable<object[]> InvalidJsonMockInputFiles = new List<object[]>
        {
            new object[]
            {
                new Dictionary<string, MockFileData>
                {
                    ["sample3.json"] = new MockFileData(@"<json>File</json>")
                },
                "Unexpected character encountered while parsing value"
            },
            new object[]
            {
                new Dictionary<string, MockFileData>
                {
                    ["sample4.json"] = new MockFileData(@"{ ""prop"": ""val"" ")
                },
                "Unexpected end of content while loading JObject"
            },
            new object[]
            {
                new Dictionary<string, MockFileData>
                {
                    ["sample5.json"] = new MockFileData(@"{ ""prop"" = ""val"" }")
                },
                "Invalid character after parsing property name. Expected ':' but got: ="
            },
            new object[]
            {
                new Dictionary<string, MockFileData>
                {
                    ["sample6.json"] = new MockFileData(@"{ ""prop"": { ""innerProp"": {""bad value""} } }")
                },
                "Invalid character after parsing property name. Expected ':' but got: }"
            }
        };

        private static Dictionary<string, MockFileData> GetOneSampleInputFile()
        {
            return new Dictionary<string, MockFileData>
            {
                {
                    "sample1.json", new MockFileData(@"
{
  ""prop1"": ""val1"",
  ""prop2"": ""val2"",
}
")
                }
            };
        }

        private static Dictionary<string, MockFileData> GetNestedMockInputFile()
        {
            var mockFiles = new Dictionary<string, MockFileData>
            {
                {
                    "subdir/sample2.json", new MockFileData(@"
{
  ""prop1"": ""val1"",
  ""prop2"": ""val2"",
}
")
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
