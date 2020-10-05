using System;
using System.Collections.Generic;
using System.Text;
using CaptainHook.Api.Client.Models;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscriberVarsReplacerTests
    {
        [Fact, IsUnit]
        public void Replace_Vars_ReturnsValidString()
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

            var result = new SubscriberTemplateReplacer().Replace(TemplateReplacementType.Vars, JObjectWithVars, varsDictionary);

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


        //        [Theory, IsUnit]
        //        [InlineData("https://ci-sample.url/api/doSomething")]
        //        [InlineData("https://test-sample.url/api/doSomething")]
        //        public void ReplaceVars_StringValues_ReplacesCorrectValue(string varValue)
        //        {
        //            // Arrange
        //            var input = JObject.Parse(@"{
        //    ""eventName"": ""sample.event"",
        //    ""subscriberName"": ""invoicing"",
        //    ""subscriber"": {  
        //        ""webhooks"": {      
        //            ""endpoints"": [
        //                {
        //                    ""uri"": ""{vars:retailer-url}"",
        //                    ""httpVerb"": ""POST"",
        //                    ""selector"": ""*""
        //                }
        //            ]
        //        }
        //    }
        //}");

        //            var subscriberVarsReplacer = new SubscriberTemplateReplacer(TemplateReplacementType.Vars);
        //            var vars = new Dictionary<string, JToken>
        //            {
        //                {"retailer-url", JToken.Parse($@"""{varValue}""")}
        //            };

        //            // Act
        //            var returnJObject = subscriberVarsReplacer.Replace(input, vars);

        //            // Assert
        //            returnJObject.SelectToken("$.subscriber.webhooks.endpoints[0].uri")!.ToString().Should().Be(varValue);
        //        }
    }
}
