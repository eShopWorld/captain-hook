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
    public class JsonTemplateValuesReplacerParamsTests
    {
        private readonly JsonTemplateValuesReplacer _jsonTemplateValuesReplacer;

        public JsonTemplateValuesReplacerParamsTests()
        {
            _jsonTemplateValuesReplacer = new JsonTemplateValuesReplacer();
        }

        [Fact, IsUnit]
        public void Replace_WhenMissingValueInDictionary_ReturnsError()
        {
            var paramsDictionary = new Dictionary<string, JToken>
            {
                ["host-uri"] = "eshop.com",
                ["scopes"] = "scope1",
            };

            var result = _jsonTemplateValuesReplacer.Replace(TemplateReplacementType.Params, JObjectWithParams, paramsDictionary);

            result.IsError.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void Replace_WhenTemplateAndDictionaryAreValid_ReturnsValidString()
        {
            var paramsDictionary = new Dictionary<string, JToken>
            {
                ["host-uri"] = "eshop.com",
                ["auth-uri"] = "authentication.com",
                ["scopes"] = "scope1",
            };

            var result = _jsonTemplateValuesReplacer.Replace(TemplateReplacementType.Params, JObjectWithParams, paramsDictionary);

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
                                    Uri = "https://blah.eshop.com/api/v1/stuff",
                                    HttpVerb = "POST",
                                    Selector = "*",
                                    Authentication = new CaptainHookContractOidcAuthenticationDto
                                    {
                                        Type = "OIDC",
                                        Uri = "https://blah.authentication.com/connect/token",
                                        ClientId = "clientId",
                                        ClientSecretKeyName = "secret-key",
                                        Scopes = new List<string>
                                        {
                                            "scope1"
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
            var result = _jsonTemplateValuesReplacer.Replace(TemplateReplacementType.Params, source, replacements);

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

        private static string JObjectWithParams =>
            @"{
  ""eventName"": ""event"",
  ""subscriberName"": ""subscriber"",
  ""subscriber"": {  
    ""webhooks"": {      
      ""endpoints"": [
        {
          ""uri"": ""https://blah.{params:host-uri}/api/v1/stuff"",
          ""httpVerb"": ""POST"",
          ""authentication"": {     
            ""clientId"": ""clientId"",
            ""uri"": ""https://blah.{params:auth-uri}/connect/token"",
            ""clientSecretKeyName"": ""secret-key"",
            ""scopes"": [
              ""{params:scopes}""
            ],
            ""type"": ""OIDC""
          },
          ""selector"": ""*""
        }
      ]
    }
  }
}";
    }
}
