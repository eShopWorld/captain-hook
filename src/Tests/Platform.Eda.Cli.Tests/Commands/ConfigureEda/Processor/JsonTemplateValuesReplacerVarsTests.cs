using System.Collections.Generic;
using CaptainHook.Api.Client.Models;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class JsonTemplateValuesReplacerVarsTests
    {
        private readonly JsonTemplateValuesReplacer _jsonVarsValuesReplacer;

        public JsonTemplateValuesReplacerVarsTests()
        {
            _jsonVarsValuesReplacer = new JsonTemplateValuesReplacer();
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
            var returnJObject = _jsonVarsValuesReplacer.Replace(TemplateReplacementType.Vars, input, vars);

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
            var result = _jsonVarsValuesReplacer.Replace(TemplateReplacementType.Vars, source, replacements);

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

        [Fact, IsUnit]
        public void Replace_ReplacementIsObjectButTemplateIsValue_ReturnsError()
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
                            ""uri"": ""https://order-transaction-api-{selector}.{vars:sts-settings}/api/v2.0/WebHook/ClientOrderFailureMethod"",
                            ""selector"": ""*"",
                            ""httpVerb"": ""POST""
                          },
                        ]
                      }
                    }
            ";

            // Act
            var result = _jsonVarsValuesReplacer.Replace(TemplateReplacementType.Vars, source, replacements);

            // Assert
            result.IsError.Should().BeTrue();
            result.Error.Message.Should().Be("Vars replacement error. 'sts-settings' is defined as an object but used as value.");
        }

        [Fact, IsUnit]
        public void Replace_VarsContainingParams_ReturnsValidString()
        {
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["webhook-uri"] = "https://blah.{params:host-uri}/api/v1/stuff",
                ["webhook-auth"] = JToken.Parse(@"{""clientId"": ""clientId"",
            ""uri"": ""https://blah.{params:auth-uri}/connect/token"",
            ""clientSecretKeyName"": ""secret-key"",
            ""scopes"": [
              ""{params:scopes}""
            ],
            ""type"": ""OIDC""}"),
            };

            var result = _jsonVarsValuesReplacer.Replace(TemplateReplacementType.Vars, JObjectWithVars, varsDictionary);

            var expected = new PutSubscriberRequest
            {
                SubscriberName = "subscriber",
                EventName = "event",
                Subscriber = new CaptainHookContractSubscriberDto
                {
                    Webhooks = new CaptainHookContractWebhooksDto
                    {
                        Endpoints = new List<CaptainHookContractEndpointDto>
                            {
                                new CaptainHookContractEndpointDto
                                {
                                    Uri = "https://blah.{params:host-uri}/api/v1/stuff",
                                    HttpVerb = "POST",
                                    Selector = "*",
                                    Authentication = new CaptainHookContractOidcAuthenticationDto
                                    {
                                        Type = "OIDC",
                                        Uri = "https://blah.{params:auth-uri}/connect/token",
                                        ClientId = "clientId",
                                        ClientSecretKeyName = "secret-key",
                                        Scopes = new List<string>
                                        {
                                            "{params:scopes}"
                                        }
                                    }
                                }
                            }
                    }
                }
            };

            var serializeObject = JsonConvert.SerializeObject(expected, Formatting.Indented, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            
            result.Data.ToLowerInvariant().Replace(" ", "").Should().BeEquivalentTo(serializeObject.ToLowerInvariant().Replace(" ", ""));
        }

        private static string JObjectWithVars =>
   @"{  
  ""eventName"": ""event"",
  ""subscriberName"": ""subscriber"",
  ""subscriber"": {  
    ""webhooks"": {      
      ""endpoints"": [
        {
          ""uri"": ""{vars:webhook-uri}"",
          ""httpVerb"": ""POST"",
          ""authentication"": ""{vars:webhook-auth}"",
          ""selector"": ""*""
        }
      ]
    }
  }
}";


    }
}
