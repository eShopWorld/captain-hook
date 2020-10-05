using System;
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

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class SubscriberFileParserTests
    {        
        // TODO (Nikhil): Enable tests after refactoring is done
        /* 
        private const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly SubscriberFileParser _subscriberFileParser;
        private readonly MockFileSystem _mockFileSystem;

        public SubscriberFileParserTests()
        {
            _mockFileSystem = new MockFileSystem(GetOneSampleInputFile(), MockCurrentDirectory);
            _subscriberFileParser = new SubscriberFileParser(_mockFileSystem);
        }

        [Theory, IsUnit]
        [InlineData(null, "Value cannot be null. (Parameter 'path')")]
        [InlineData("", "Empty file name is not legal. (Parameter 'path')")]
        [InlineData("a-nonexistent-path", "Could not find file 'a-nonexistent-path'.")]
        public void ParseFile_WhenPathEmptyOrNull_ThrowsException(string testFilePath, string errorMessage)
        {
            var result = _subscriberFileParser.ParseFile(testFilePath);
            result.IsError.Should().BeTrue();
            result.Error.Should().Be(errorMessage);
        }

        [Fact, IsUnit]
        public void ParseFile_WithValidFile_ReturnsValidObject()
        {
            // Act
            var result = _subscriberFileParser.ParseFile(Path.Combine(MockCurrentDirectory, "sample1.json"));

            // Assert
            var expected = new PutSubscriberFile
            {
                Request = new PutSubscriberRequest
                {
                    SubscriberName = "test-sub",
                    EventName = "test-event",
                }
            };

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Fact, IsUnit]
        public void ParseFile_WithFileInSubDirectory_ReturnsValidObject()
        {
            var fs = new MockFileSystem(GetNestedMockInputFile(), MockCurrentDirectory);
            var subscriberFileParser = new SubscriberFileParser(fs);

            // Act
            var result = subscriberFileParser.ParseFile(fs.AllFiles.First());

            // Assert
            var expected = new PutSubscriberFile
            {
                Request = new PutSubscriberRequest
                {
                    SubscriberName = "test-sub2",
                    EventName = "test-event2"
                },
            };

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Fact, IsUnit]
        public void ParseFile_WithInvalidJson_ReturnsObjectWithErrorInformation()
        {
            // Arrange
            var subscriberFileParser = new SubscriberFileParser(new MockFileSystem(GetInvalidJsonMockInputFile(), MockCurrentDirectory));

            // Act
            var result = subscriberFileParser.ParseFile(MockCurrentDirectory);

            // Assert
            var expected = new PutSubscriberFile
            {
                Request = null,
                Error = "Error reading JObject from JsonReader. Path '', line 0, position 0."
            };

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));
        }

        [Fact, IsUnit]
        public void ParseFile_JsonWithCallbacksDlqHooks_ReturnsValidObject()
        {
            // Arrange
            var files = GetSampleInputFileWithDlq();
            var subscriberFileParser = new SubscriberFileParser(new MockFileSystem(files, MockCurrentDirectory));

            // Act
            var result = subscriberFileParser.ParseFile("sample1.json");

            // Assert
            var expected = new PutSubscriberFile
            {
                Request = new PutSubscriberRequest
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
                }
            };

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.Name == "Authentication")
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        [Fact, IsUnit]
        public void GetFileVars_ValidVars_ReturnsValidObject()
        {
            // Arrange
            var jObject = GetJObjectWithVars();

            // Act
            var result = _subscriberFileParser.GetFileVars(jObject);

            // Assert
            var expected = new Dictionary<string, Dictionary<string, string>> {
                                {
                                    "leviss-url", new Dictionary<string, string>
                                    {
                                        {"prod", "https://blah.blah.company.com/Order/Createresponse"},
                                        {"sand", "https://blah.blah.company.test.com/v1/Order/Createresponse"}
                                    }
                                },
                                {
                                    "leviss-auth", new Dictionary<string, string>
                                    {
                                        {
                                            "prod", @"{""type"":""Basic"",""username"":""abc"",""passwordKeyName"":""man--something""}"
                                        },
                                        {
                                            "sand", @"{""type"":""Basic"",""username"":""def"",""passwordKeyName"":""man--other""}"
                                        }
                                    }
                                },
                                {
                                    "sts-settings", new Dictionary<string, string>
                                    {
                                        {
                                            "prod", @"{""type"":""OIDC"",""scopes"":[""any.valid.scope""]}"
                                        },
                                        {
                                            "sand", @"{""type"":""OIDC"",""scopes"":[""any.valid.scope""]}"
                                        }
                                    }
                                }
                            };

            result.Should().BeEquivalentTo(expected);
            jObject.Should().NotContainKey("vars");
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

        private static Dictionary<string, MockFileData> GetNestedMockInputFile()
        {
            var mockFiles = new Dictionary<string, MockFileData>
            {
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

        private static JObject GetJObjectWithVars()
        {
            return JObject.Parse(@"
{
  ""vars"": {
    ""leviss-url"": { 
      ""prod"": ""https://blah.blah.company.com/Order/Createresponse"",
      ""sand"": ""https://blah.blah.company.test.com/v1/Order/Createresponse""
    },
    ""leviss-auth"": {
      ""prod"": {
        ""type"": ""Basic"",
        ""username"": ""abc"",
        ""passwordKeyName"": ""man--something""
      },
      ""sand"": {
        ""type"": ""Basic"",
        ""username"": ""def"",
        ""passwordKeyName"": ""man--other""
      }
    },
    ""sts-settings"": {     
      ""prod,sand"": {
        ""type"": ""OIDC"",
        ""scopes"": [
          ""any.valid.scope""
        ]
      }
    }
  },
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
");
        }
        */
    }
}
