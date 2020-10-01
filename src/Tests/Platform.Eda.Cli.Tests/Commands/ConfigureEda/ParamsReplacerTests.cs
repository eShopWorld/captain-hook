using System.Collections.Generic;
using System.IO.Abstractions;
using CaptainHook.Api.Client.Models;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ParamsReplacerTests
    {        

        [Fact, IsUnit]
        public void ParseFile_WithValidFile_ReturnsValidObject()
        {
            var paramsDictionary = new Dictionary<string, string>();

            var result = new ParamsReplacer().BuildRequest(JObjectWithParams, paramsDictionary);

            var expected = new PutSubscriberFile
            {
                Request = new PutSubscriberRequest
                {
                    SubscriberName = "subscriber",
                    EventName = "event",
                    Subscriber = new CaptainHookContractSubscriberDto()
                    {

                    }
                }
            };

            result.Should().BeEquivalentTo(expected, opt => opt
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(CaptainHookContractSubscriberDto))
                .Excluding(info => info.SelectedMemberInfo.MemberType == typeof(FileInfoBase)));

        }

        private static JObject JObjectWithParams =>
            JObject.Parse(@"
{
  ""vars"": {
    ""retailer-url"": { 
      ""ci,prep,sand,prod"": ""https://blah.{params:evo-host}/api/v1/stuff""      
    },
    ""sts-settings"": {     
      ""ci,prep,sand,prod"": {
        ""type"": ""OIDC"",
        ""uri"": ""https://blah.{params:evo-host}/connect/token"",
        ""clientId"": ""clientId"",
        ""clientSecretKey"": ""secret-key"",
        ""scopes"": [
          ""scope1""
        ]
      }
    }
  },  
  ""eventName"": ""event"",
  ""subscriberName"": ""subscriber"",
  ""subscriber"": {  
    ""webhooks"": {      
      ""endpoints"": [
        {
          ""uri"": ""{vars:retailer-url}"",
          ""httpVerb"": ""POST"",
          ""selector"": ""*"",
          ""authentication"": ""{vars:sts-settings}""
        }
      ]
    }
  }
}
");
    }
}
