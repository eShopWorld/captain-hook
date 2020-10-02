using System;
using System.Collections.Generic;
using System.Text;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscriberVarsReplacerTests
    {
        [Theory, IsUnit]
        [InlineData("https://ci-sample.url/api/doSomething")]
        [InlineData("https://test-sample.url/api/doSomething")]
        public void ReplaceVars_StringValues_ReplacesCorrectValue(string varValue)
        {
            // Arrange
            var input = JObject.Parse(@"{
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
}");

            var subscriberVarsReplacer = new SubscriberTemplateReplacer("vars");
            var vars = new Dictionary<string, JToken>
            {
                {"retailer-url", JToken.Parse($@"""{varValue}""")}
            };

            // Act
            var returnJObject = subscriberVarsReplacer.Replace(input, vars);

            // Assert
            returnJObject.SelectToken("$.subscriber.webhooks.endpoints[0].uri")!.ToString().Should().Be(varValue);
        }
    }
}
