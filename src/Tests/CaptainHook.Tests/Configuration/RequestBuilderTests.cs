using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class RequestBuilderTests
    {
        [IsUnit]
        [Theory]
        [InlineData("GET")]
        [InlineData("POST")]
        [InlineData("PUT")]
        [InlineData("DELETE")]
        public void IdempotencyKeyHeaderTests_Get(string method)
        {
            var config = new WebhookConfig
            {
                Name = "Webhook2",
                EventType =  "eventType",
                HttpMethod = HttpMethod.Get,
                Uri = "https://blah.blah.eshopworld.com/webhook/",
                AuthenticationConfig = new OidcAuthenticationConfig(),
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
                            }
                        }
            };
            config.HttpMethod = new HttpMethod(method);

            var messageData = new MessageData("blah", "blahtype", "blahsubscriber", "blahReplyTo", false) { ServiceBusMessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString() };

            var headers = new RequestBuilder(Mock.Of<IBigBrother>()).GetHttpHeaders(config, messageData);
            Assert.True(headers.RequestHeaders.ContainsKey(Constants.Headers.IdempotencyKey));
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(UriData))]
        public void UriConstructionTests(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = new RequestBuilder(Mock.Of<IBigBrother>()).BuildUri(config, payload);

            Assert.Equal(new Uri(expectedUri), uri);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(PayloadData))]
        public void PayloadConstructionTests(
            WebhookConfig config,
            string sourcePayload,
            Dictionary<string, object> data,
            string expectedPayload)
        {
            var requestPayload = new RequestBuilder(Mock.Of<IBigBrother>()).BuildPayload(config, sourcePayload, data);

            Assert.Equal(expectedPayload, requestPayload);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(HttpVerbData))]
        public void HttpVerbSelectionTests(
            WebhookConfig config,
            string sourcePayload,
            HttpMethod expectedVerb)
        {
            var selectedVerb = new RequestBuilder(Mock.Of<IBigBrother>()).SelectHttpMethod(config, sourcePayload);

            Assert.Equal(expectedVerb, selectedVerb);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(AuthenticationSchemeData))]
        public void AuthenticationSchemeSelectionTests(
            WebhookConfig config,
            string sourcePayload,
            AuthenticationType expectedAuthenticationType)
        {
            var authenticationConfig = new RequestBuilder(Mock.Of<IBigBrother>()).GetAuthenticationConfig(config, sourcePayload);

            Assert.Equal(expectedAuthenticationType, authenticationConfig.AuthenticationConfig.Type);
        }

        public static IEnumerable<object[]> UriData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook1",
                            HttpMethod = HttpMethod.Post,
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
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
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
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand3.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
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
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook5",
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
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
                    "{\"OrderCode\":\"DEV13:00026804\", \"BrandType\":\"Brand1\"}",
                    "https://blah.blah.brand1.eshopworld.com/webhook/DEV13%3A00026804"
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
                        HttpMethod = HttpMethod.Post,
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
                        HttpMethod = HttpMethod.Post,
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
                        HttpMethod = HttpMethod.Post,
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
                        HttpMethod = HttpMethod.Post,
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
                        HttpMethod = HttpMethod.Post,
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
                    HttpMethod.Post
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook3",
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
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
                    HttpMethod.Put
                }
            };

        public static IEnumerable<object[]> AuthenticationSchemeData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook1",
                        HttpMethod = HttpMethod.Post,
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
                    AuthenticationType.None
                },
                new object[]
                {
                    new WebhookConfig
                    {
                        Name = "Webhook2",
                        HttpMethod = HttpMethod.Post,
                        Uri = "https://blah.blah.eshopworld.com/webhook/",
                        AuthenticationConfig = new OidcAuthenticationConfig(),
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
                            }
                        }
                    },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    AuthenticationType.OIDC
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook3",
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
                                            Selector = "Brand2",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.Basic
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\"}",
                    AuthenticationType.Basic
                },
                new object[]
                {
                    new WebhookConfig
                        {
                            Name = "Webhook4",
                            HttpMethod = HttpMethod.Post,
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
                                            HttpMethod = HttpMethod.Post,
                                            Selector = "Brand1",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.None
                                            }
                                        },
                                        new WebhookConfigRoute
                                        {
                                            Uri = "https://blah.blah.brand2.eshopworld.com/webhook",
                                            HttpMethod = HttpMethod.Put,
                                            Selector = "Brand2",
                                            AuthenticationConfig = new AuthenticationConfig
                                            {
                                                Type = AuthenticationType.Basic
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand1\"}",
                    AuthenticationType.None
                }
            };   
    }
}
