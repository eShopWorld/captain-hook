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
            var uri = new RequestBuilder().BuildUri(config, payload);

            Assert.Equal(expectedUri, uri);
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(PayloadData))]
        public void PayloadConstructionTests(
            WebhookConfig config,
            string sourcePayload,
            Dictionary<string, object> data,
            string expectedPayload)
        {
            var requestPayload = new RequestBuilder().BuildPayload(config, sourcePayload, data);

            Assert.Equal(expectedPayload, requestPayload);
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(HttpVerbData))]
        public void HttpVerbSelectionTests(
            WebhookConfig config,
            string sourcePayload,
            HttpVerb expectedVerb)
        {
            var selectedVerb = new RequestBuilder().SelectHttpVerb(config, sourcePayload);

            Assert.Equal(expectedVerb, selectedVerb);
        }

        public static IEnumerable<object[]> UriData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook1",
                            HttpVerb = HttpVerb.Post,
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
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
                            HttpVerb = HttpVerb.Post,
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
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
                                        Path = "BrandType"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        RuleAction = RuleAction.Route
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Put,
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
                                        Path = "OrderConfirmationRequestDto"
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
                            HttpVerb = HttpVerb.Post,
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
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
                                        Path = "BrandType"
                                    },
                                    Destination = new ParserLocation
                                    {

                                        RuleAction = RuleAction.Route
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Put,
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
                                        Path = "BrandType"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        RuleAction = RuleAction.Route
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand3.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Put,
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
                    "https://blah.blah.brand3.eshopworld.com/webhook"
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
                        HttpVerb = HttpVerb.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "InnerModel"
                                },
                                Destination = new ParserLocation
                                {
                                    RuleAction = RuleAction.Replace,
                                    Type = DataType.Model
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object> (),
                    "{\"Msg\":\"Buy this thing\"}"
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        HttpVerb = HttpVerb.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "InnerModel"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "Payload",
                                    Type = DataType.Model
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "OrderCode"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "OrderCode",
                                    Type = DataType.Model
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>(),
                    "{\"Payload\":{\"Msg\":\"Buy this thing\"},\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\"}"
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        HttpVerb = HttpVerb.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "OrderCode"
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "OrderCode"
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Type = DataType.HttpStatusCode,
                                    Location = Location.HttpStatusCode
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "HttpStatusCode",
                                    Type = DataType.Property
                                }
                            },
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Type = DataType.HttpContent
                                },
                                Destination = new ParserLocation
                                {
                                    Path = "Content",
                                    Type = DataType.Model
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>{{"HttpStatusCode", 200}, {"HttpResponseContent", "{\"Msg\":\"Buy this thing\"}" } },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\",\"HttpStatusCode\":200,\"Content\":{\"Msg\":\"Buy this thing\"}}"
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        HttpVerb = HttpVerb.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
                                    Path = "InnerModel"
                                },
                                Destination = new ParserLocation
                                {
                                    Type = DataType.Model
                                }
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\", \"InnerModel\": {\"Msg\":\"Buy this thing\"}}",
                    new Dictionary<string, object>(),
                    "{\"Msg\":\"Buy this thing\"}"
                },
            };

        public static IEnumerable<object[]> HttpVerbData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        HttpVerb = HttpVerb.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        WebhookRequestRules = new List<WebhookRequestRule>
                        {
                            new WebhookRequestRule
                            {
                                Source = new ParserLocation
                                {
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
                    HttpVerb.Post
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook3",
                            HttpVerb = HttpVerb.Post,
                            Uri = "https://blah.blah.eshopworld.com/webhook/",
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
                                        Path = "OrderCode"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.Uri,
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Source = new ParserLocation
                                    {
                                        Path = "BrandType"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        RuleAction = RuleAction.Route
                                    },
                                    Routes = new List<WebhookConfigRoute>
                                    {
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand1.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpVerb = HttpVerb.Put,
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
                    HttpVerb.Put
                }
            };
    }
}
