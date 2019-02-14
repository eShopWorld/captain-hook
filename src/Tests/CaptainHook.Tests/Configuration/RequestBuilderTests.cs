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
        public void UriConstructionTests(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = RequestBuilder.BuildUri(config, payload);

            Assert.Equal(expectedUri, uri);
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(PayloadData))]
        public void PayloadConstructionTests(
            WebhookConfig config,
            string sourcePayload,
            Dictionary<string, string> data,
            string expectedPayload)
        {
            var requestPayload = RequestBuilder.BuildPayload(config, sourcePayload, data);

            Assert.Equal(expectedPayload, requestPayload);
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
                                    Source = new ParserLocation
                                    {
                                        Location = Location.MessageBody,
                                        Path = "OrderCode"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.Uri
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook2",
                            HttpVerb = "POST",
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
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
                                        Location = Location.MessageBody
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = "POST",
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = "PUT",
                                            Selector = "Brand2",
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
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.PayloadBody
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.brand1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook3",
                            HttpVerb = "POST",
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
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
                                        Location = Location.MessageBody
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = "POST",
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = "PUT",
                                            Selector = "Brand2",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    "https://blah.blah.brand2.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook4",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
                                        Path = "BrandType",
                                        Location = Location.MessageBody
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = "POST",
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = "PUT",
                                            Selector = "Brand2",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    "https://blah.blah.brand2.eshopworld.com/webhook"
                }
            };

        public static IEnumerable<object[]> PayloadData =>
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
                                Source = new ParserLocation
                                {
                                    Location = Location.MessageBody,
                                    Path = "InnerModel"
                                },
                                Destination = new ParserLocation
                                {
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Replace
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, string> (),
                    "{\"Msg\":\"Buy this thing\"}"
                },
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
                                Source = new ParserLocation
                                {
                                    Location = Location.MessageBody,
                                    Path = "InnerModel"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "Payload",
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Add
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Location = Location.MessageBody,
                                    Path = "OrderCode"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "OrderCode",
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Add
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, string>(),
                    "{\"Payload\":{\"Msg\":\"Buy this thing\"},\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\"}"
                },
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
                                Source = new ParserLocation
                                {
                                    Location = Location.MessageBody,
                                    Path = "OrderCode"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "OrderCode",
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Add
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Type = DataType.HttpStatusCode,
                                    Location = Location.StatusCode
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "StatusCode",
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Add
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Type = DataType.HttpContent,
                                    Location = Location.PayloadBody
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "Content",
                                    Location = Location.PayloadBody,
                                    RuleAction = RuleAction.Add
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, string>{{"HttpStatusCode", "200"}, {"HttpResponseContent", "{\"Msg\":\"Buy this thing\"}" } },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\",\"StatusCode\":\"200\",\"Content\":{\"Msg\":\"Buy this thing\"}}"
                }
            };
    }
}
