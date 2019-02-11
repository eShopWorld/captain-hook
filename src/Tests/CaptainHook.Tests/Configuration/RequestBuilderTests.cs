using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class RequestBuilderTests
    {
        [IsLayer0]
        [Theory]
        [MemberData(nameof(UriData))]
        public void BuildUriTest(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = RequestBuilder.BuildUri(config, payload);

            Assert.Equal(expectedUri, uri);
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(PayloadData))]
        public void BuildPayloadTest(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = RequestBuilder.BuildPayload(config, payload);

            Assert.Equal(expectedUri, uri);
        }

        public static IEnumerable<object[]> UriData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook1",
                            HttpVerb = "POST",
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Name = "OrderCode",
                                    Source = new ParserLocation
                                    {
                                        Location = Location.MessageBody,
                                        Path = "OrderCode"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.Uri
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
                                        Path = "BrandType",
                                        Location = Location.PayloadBody
                                    },
                                    Routes = new List<WebhookConfigRoutes>
                                    {
                                        new WebhookConfigRoutes
                                        {
                                            Uri = "https://blah.blah.brandytype.eshopworld.com",
                                            HttpVerb = "POST",
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        }
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
                                        Path = "OrderConfirmationRequestDto",
                                        Location = Location.PayloadBody
                                    },
                                    Type = QueryRuleTypes.Model,
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.PayloadBody
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"GOC\"}",
                    "https://blah.blah.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                }
            };

        public static IEnumerable<object[]> PayloadData =>
            new List<object[]>
            {
                new object[]
                {

                }
            };
    }
}
