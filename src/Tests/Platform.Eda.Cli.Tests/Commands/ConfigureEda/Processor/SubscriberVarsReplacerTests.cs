using System;
using System.Collections.Generic;
using System.Text;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Kusto.Cloud.Platform.Data;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscriberVarsReplacerTests
    {
        private readonly SubscriberTemplateReplacer _subscriberVarsReplacer;

        public SubscriberVarsReplacerTests()
        {
            _subscriberVarsReplacer = new SubscriberTemplateReplacer();
        }

        [Theory, IsUnit]
        [InlineData("https://ci-sample.url/api/retailer")]
        [InlineData("https://test-sample.url/api/retailer")]
        public void Replace_StringValues_ReplacesCorrectValue(string varValue)
        {
            // Arrange
            var input = @"{
    ""eventName"": ""sample.event"",
    ""subscriberName"": ""invoicing"",
    ""subscriber"": {  
        ""webhooks"": {      
            ""endpoints"": [
                {
                    ""uri"": ""{vars:retailer-url}"",
                    ""httpVerb"": ""POST"",
                    ""selector"": ""*""
                }
            ]
        }
    }
}";

            var vars = new Dictionary<string, JToken>
            {
                {"retailer-url", JToken.Parse($@"""{varValue}""")}
            };

            // Act
            var returnJObject = _subscriberVarsReplacer.Replace(TemplateReplacementType.Vars, input, vars);

            // Assert
            returnJObject.Data.Should().Be($@"{{
    ""eventName"": ""sample.event"",
    ""subscriberName"": ""invoicing"",
    ""subscriber"": {{  
        ""webhooks"": {{      
            ""endpoints"": [
                {{
                    ""uri"": ""{varValue}"",
                    ""httpVerb"": ""POST"",
                    ""selector"": ""*""
                }}
            ]
        }}
    }}
}}");
        }

        [Fact, IsUnit]
        public void Replace_WhenReplacementIsString_ReplacedWithString()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                {"evo-host", (JToken) "sandbox.eshopworld.com"}
            };

            const string source = @"
                    {
                      ""eventName"": ""eshopworld.platform.events.oms.ordercancelfailedevent"",
                      ""subscriberName"": ""retailers"",
                      ""webhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""payloadTransform"": ""Response"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2.0/WebHook/ClientOrderFailureMethod"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          },
                        ]
                      },
                      ""callbacks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2/EdaResponse/ExternalEdaResponse"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      },
                      ""dlqhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2/DlqRequest/ExternalRequest"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      }
                    }
            ";

            // Act
            var result = _subscriberVarsReplacer.Replace(TemplateReplacementType.Params, source, replacements);

            // Assert
            using (new AssertionScope())
            {
                result.IsError.Should().BeFalse();
                result.Data.Should().Be(@"
                    {
                      ""eventName"": ""eshopworld.platform.events.oms.ordercancelfailedevent"",
                      ""subscriberName"": ""retailers"",
                      ""webhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""payloadTransform"": ""Response"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2.0/WebHook/ClientOrderFailureMethod"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          },
                        ]
                      },
                      ""callbacks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2/EdaResponse/ExternalEdaResponse"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      },
                      ""dlqhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2/DlqRequest/ExternalRequest"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      }
                    }
            ");
            }
        }

        [Fact, IsUnit]
        public void Replace_ReplacementIsObject_ReplacedWithObject()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                {"sts-settings", JToken.Parse("{ \"type\": \"OIDC\" } ")},
            };

            var source = @"
                    {
                      ""eventName"": ""eshopworld.platform.events.oms.ordercancelfailedevent"",
                      ""subscriberName"": ""retailers"",
                      ""webhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""payloadTransform"": ""Response"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2.0/WebHook/ClientOrderFailureMethod"",
                            ""selector"": ""*"",
                            ""authentication"": ""{vars:sts-settings}"",
                            ""httpVerb"": ""POST""
                          },
                        ]
                      },
                      ""callbacks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2/EdaResponse/ExternalEdaResponse"",
                            ""selector"": ""*"",
                            ""authentication"": ""{vars:sts-settings}"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      },
                      ""dlqhooks"": {
                        ""selectionRule"": ""$.TenantCode"",
                        ""endpoints"": [
                          {
                            ""uri"": ""https://order-transaction-api-{selector}.{params:evo-host}/api/v2/DlqRequest/ExternalRequest"",
                            ""selector"": ""*"",
                            ""authentication"": ""{vars:sts-settings}"",
                            ""httpVerb"": ""POST""
                          }
                        ]
                      }
                    }
            ";

            // Act
            var result = _subscriberVarsReplacer.Replace(TemplateReplacementType.Vars, source, replacements);

            // Assert
            result.IsError.Should().BeFalse();
            var resultJObject = JObject.Parse(result.Data);
            resultJObject.Should().NotBeNull();

            using (new AssertionScope())
            {
                resultJObject["webhooks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should().Be("OIDC");
                resultJObject["callbacks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should()
                    .Be("OIDC");
                resultJObject["dlqhooks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should().Be("OIDC");
            }
        }


        public static IEnumerable<object[]> TestDictionaries = new List<object[]>
        {
            new object[] { new Dictionary<string, JToken>()},
            new object[] {null}
        };

        [Fact, IsUnit]
        public void Replace_ReplacementIsNull_ThenReturnsError()
        {
            var input = @"{
    ""eventName"": ""sample.event"",
    ""subscriberName"": ""invoicing"",
    ""subscriber"": {  
        ""webhooks"": {      
            ""endpoints"": [
                {
                    ""uri"": ""{vars:retailer-url}"",
                    ""httpVerb"": ""POST"",
                    ""selector"": ""*""
                }
            ]
        }
    }
}";
            var result = _subscriberVarsReplacer.Replace(TemplateReplacementType.Params, input, null);
            result.IsError.Should().BeTrue();
        }
    }
}