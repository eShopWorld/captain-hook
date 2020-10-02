using System;
using System.Collections.Generic;
using System.Text;
using Eshopworld.Tests.Core;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class SubscriberVarsReplacerTests
    {
        [Fact, IsUnit]
        public void Test1()
        {
            var input = JObject.Parse(@"{
    ""vars"": {
        ""retailer-url"": { 
            ""ci"": ""https://ci-sample.url/api/doSomething"",
            ""test"": ""https://test-sample.url/api/doSomething"",
            ""prep"": ""https://prep-sample.url/api/doSomething"",
            ""sand"": ""https://sand-sample.url/api/doSomething"",
            ""prod"": ""https://prod-sample.url/api/doSomething""    
        }
    },  
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
            JsonVarsParser parser = new JsonVarsParser();
            var vars = parser.GetFileVars(input, "ci");
            var subscriberVarsReplacer = new SubscriberVarsReplacer();
            var returnobJObject = subscriberVarsReplacer.ReplaceVars(input, vars);
            Assert.True(true);
        }
    }
}
