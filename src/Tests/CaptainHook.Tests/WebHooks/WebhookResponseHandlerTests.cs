using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.Tests.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.WebHooks
{
    public class WebhookResponseHandlerTests
    {
        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(WebHookCallData))]
        public async Task CheckWebhookCall(EventHandlerConfig config, MessageData messageData, string expectedUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var mockWebHookRequestWithCallback = mockHttpHandler.When(HttpMethod.Post, expectedUri)
                .WithContentType("application/json", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClient = mockHttpHandler.ToHttpClient();

            var mockAuthHandler = new Mock<IAcquireTokenHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    mockAuthHandler.Object,
                    new RequestBuilder(),
                    mockBigBrother.Object,
                    httpClient,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                mockAuthHandler.Object,
                new RequestBuilder(),
                mockBigBrother.Object,
                httpClient,
                config);

            await webhookResponseHandler.Call(messageData);

            mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.Exactly(1));
            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequestWithCallback));
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(CallbackCallData))]
        public async Task CheckCallbackCall(EventHandlerConfig config, MessageData messageData, string expectedWebHookUri, string expectedCallbackUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var mockWebHookRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClient = mockHttpHandler.ToHttpClient();

            var mockAuthHandler = new Mock<IAcquireTokenHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    mockAuthHandler.Object,
                    new RequestBuilder(),
                    mockBigBrother.Object,
                    httpClient,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                mockAuthHandler.Object,
                new RequestBuilder(),
                mockBigBrother.Object,
                httpClient,
                config);

            await webhookResponseHandler.Call(messageData);

            mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.Exactly(1));
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.IsAny<string>()), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequest));
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(GoodMultiRouteCallData))]
        public async Task GoodCheckMultiRouteSelection(EventHandlerConfig config, MessageData messageData, string expectedWebHookUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var multiRouteCall = mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClient = mockHttpHandler.ToHttpClient();

            var mockAuthHandler = new Mock<IAcquireTokenHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    mockAuthHandler.Object,
                    new RequestBuilder(),
                    mockBigBrother.Object,
                    httpClient,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                mockAuthHandler.Object,
                new RequestBuilder(),
                mockBigBrother.Object,
                httpClient,
                config);

            await webhookResponseHandler.Call(messageData);

            mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.Exactly(1));
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.IsAny<string>()), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(multiRouteCall));
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(BadMultiRouteCallData))]
        public async Task BadCheckMultiRouteSelection(EventHandlerConfig config, MessageData messageData, string expectedWebHookUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var multiRouteCall = mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClient = mockHttpHandler.ToHttpClient();

            var mockAuthHandler = new Mock<IAcquireTokenHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    mockAuthHandler.Object,
                    new RequestBuilder(),
                    mockBigBrother.Object,
                    httpClient,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                mockAuthHandler.Object,
                new RequestBuilder(),
                mockBigBrother.Object,
                httpClient,
                config);

            await Assert.ThrowsAsync<Exception>(async () => await webhookResponseHandler.Call(messageData));
        }

        public static IEnumerable<object[]> WebHookCallData =>
            new List<object[]>
            {
                new object[]
                {
                    EventHandlerConfigWithSingleRoute,
                    EventHandlerTestHelper.CreateMessageDataPayload().data,
                    "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "{\"TransportModel\":\"{\\\"Name\\\":\\\"Hello World\\\"}\"}"
                }
            };

        public static IEnumerable<object[]> CallbackCallData =>
            new List<object[]>
            {
                new object[]
                {
                    EventHandlerConfigWithSingleRoute,
                    EventHandlerTestHelper.CreateMessageDataPayload().data,
                    "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "https://callback.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "{\"TransportModel\":\"{\\\"Name\\\":\\\"Hello World\\\"}\"}"
                }
            };

        public static IEnumerable<object[]> GoodMultiRouteCallData =>
            new List<object[]>
            {
                new object[]
                {
                    EventHandlerConfigWithGoodMultiRoute,
                    EventHandlerTestHelper.CreateMessageDataPayload().data,
                    "https://blah.blah.multiroute.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "{\"TransportModel\":{\"Name\":\"Hello World\"}}"
                }
            };

        public static IEnumerable<object[]> BadMultiRouteCallData =>
            new List<object[]>
            {
                new object[]
                {
                    EventHandlerConfigWithBadMultiRoute,
                    EventHandlerTestHelper.CreateMessageDataPayload().data,
                    "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "{\"TransportModel\":{\"Name\":\"Hello World\"}}"
                }
            };

        private static EventHandlerConfig EventHandlerConfigWithSingleRoute => new EventHandlerConfig
        {
            Name = "Event 1",
            Type = "blahblah",
            WebHookConfig = new WebhookConfig
            {
                Name = "Webhook1",
                HttpVerb = HttpVerb.Post,
                Uri = "https://blah.blah.eshopworld.com",
                AuthenticationConfig = new OidcAuthenticationConfig
                {
                    Type = AuthenticationType.OIDC,
                    Uri = "https://blah-blah.sts.eshopworld.com",
                    ClientId = "ClientId",
                    ClientSecret = "ClientSecret",
                    Scopes = new[] { "scope1", "scope2" }
                },
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
                            Path = "TransportModel",
                            Type = DataType.Model
                        },
                        Destination = new ParserLocation
                        {
                            Path = "TransportModel",
                            Type = DataType.String
                        }
                    }
                }
            },
            CallbackConfig = new WebhookConfig
            {
                Name = "PutOrderConfirmationEvent",
                HttpVerb = HttpVerb.Put,
                Uri = "https://callback.eshopworld.com",
                AuthenticationConfig = new AuthenticationConfig
                {
                    Type = AuthenticationType.None
                },
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
                            Path = "OrderCode",
                            Location = Location.Uri
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Type = DataType.HttpStatusCode
                        },
                        Destination = new ParserLocation
                        {
                            Path = "HttpStatusCode"
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
                            Type = DataType.String
                        }
                    }
                }
            }
        };

        private static EventHandlerConfig EventHandlerConfigWithGoodMultiRoute => new EventHandlerConfig
        {
            Name = "Event 1",
            Type = "blahblah",
            WebHookConfig = new WebhookConfig
            {
                Name = "Webhook1",
                HttpVerb = HttpVerb.Post,
                Uri = "https://blah.blah.eshopworld.com",
                AuthenticationConfig = new OidcAuthenticationConfig
                {
                    Type = AuthenticationType.OIDC,
                    Uri = "https://blah-blah.sts.eshopworld.com",
                    ClientId = "ClientId",
                    ClientSecret = "ClientSecret",
                    Scopes = new[] { "scope1", "scope2" }
                },
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
                                Uri = "https://blah.blah.multiroute.eshopworld.com",
                                HttpVerb = HttpVerb.Post,
                                Selector = "Good",
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
                            Path = "TransportModel",
                            Type = DataType.Model
                        },
                        Destination = new ParserLocation
                        {
                            Path = "TransportModel",
                            Type = DataType.Model
                        }
                    }
                }
            },
            CallbackConfig = new WebhookConfig
            {
                Name = "PutOrderConfirmationEvent",
                HttpVerb = HttpVerb.Post,
                Uri = "https://callback.eshopworld.com",
                AuthenticationConfig = new AuthenticationConfig
                {
                    Type = AuthenticationType.None
                },
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
                            Type = DataType.HttpStatusCode
                        },
                        Destination = new ParserLocation
                        {
                            Path = "HttpStatusCode"
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
                            Type = DataType.String
                        }
                    }
                }
            }
        };

        private static EventHandlerConfig EventHandlerConfigWithBadMultiRoute => new EventHandlerConfig
        {
            Name = "Event 1",
            Type = "blahblah",
            WebHookConfig = new WebhookConfig
            {
                Name = "Webhook1",
                HttpVerb = HttpVerb.Post,
                Uri = "https://blah.blah.eshopworld.com",
                AuthenticationConfig = new OidcAuthenticationConfig
                {
                    Type = AuthenticationType.OIDC,
                    Uri = "https://blah-blah.sts.eshopworld.com",
                    ClientId = "ClientId",
                    ClientSecret = "ClientSecret",
                    Scopes = new[] { "scope1", "scope2" }
                },
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
                                Uri = "https://blah.blah.multiroute.eshopworld.com",
                                HttpVerb = HttpVerb.Post,
                                Selector = "Bad",
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
                            Path = "TransportModel",
                            Type = DataType.Model
                        },
                        Destination = new ParserLocation
                        {
                            Path = "TransportModel",
                            Type = DataType.Model
                        }
                    }
                }
            },
            CallbackConfig = new WebhookConfig
            {
                Name = "PutOrderConfirmationEvent",
                HttpVerb = HttpVerb.Post,
                Uri = "https://callback.eshopworld.com",
                AuthenticationConfig = new AuthenticationConfig
                {
                    Type = AuthenticationType.None
                },
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
                            Path = "OrderCode",
                            Location = Location.Uri
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Type = DataType.HttpStatusCode,
                        },
                        Destination = new ParserLocation
                        {
                            Path = "HttpStatusCode"
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
                            Type = DataType.String
                        }
                    }
                }
            }
        };
    }
}
