using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using System.Collections.Generic;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class JsonReplacerTests
    {
        private JsonReplacer jsonReplacer = new JsonReplacer();

        [Fact, IsUnit]
        public void When_ReplacementIsString_Then_ItIsReplacedWithString()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                { "{params:evo-host}", (JToken)"sandbox.eshopworld.com" }
            };

            var source = JObject.Parse(@"
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
            ");

            // Act
            var result = jsonReplacer.Replace(source, replacements);

            // Assert
            using(new AssertionScope())
            result.IsError.Should().BeFalse();
            result.Data["webhooks"]["endpoints"][0]["uri"].Should().BeEmpty("https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2.0/WebHook/ClientOrderFailureMethod");
            result.Data["callbacks"]["endpoints"][0]["uri"].Should().BeEmpty("https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2/EdaResponse/ExternalEdaResponse");
            result.Data["dlqhooks"]["endpoints"][0]["uri"].Should().BeEmpty("https://order-transaction-api-{selector}.sandbox.eshopworld.com/api/v2/DlqRequest/ExternalRequest");
        }

        [Fact, IsUnit]
        public void When_ReplacementIsObject_Then_ItIsReplacedWithObject()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                { "{vars:sts-settings}", JToken.Parse("{ \"type\": \"OIDC\" } ") },
            };

            var source = JObject.Parse(@"
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
            ");

            // Act
            var result = jsonReplacer.Replace(source, replacements);

            // Assert
            using (new AssertionScope())
            result.IsError.Should().BeFalse();
            result.Data["webhooks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should().Be("OIDC");
            result.Data["callbacks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should().Be("OIDC");
            result.Data["dlqhooks"]["endpoints"][0]["authentication"]["type"].Value<string>().Should().Be("OIDC");
        }
    }
}
