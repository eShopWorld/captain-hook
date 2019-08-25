using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.Tests.Web.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class WebhookResponseHandlerTests
    {
        private readonly CancellationToken _cancellationToken;

        public WebhookResponseHandlerTests()
        {
            _cancellationToken = new CancellationToken();
        }

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

            var mockBigBrother = new Mock<IBigBrother>();

            var httpClients = new IndexDictionary<string, HttpClient>
            {
                {new Uri(config.WebhookConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.CallbackConfig.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Mock<IAcquireTokenHandler>().Object);

            var httpClientBuilder = new HttpClientBuilder(mockAuthHandlerFactory.Object, httpClients );
            var requestBuilder = new RequestBuilder();
            var requestLogger = new RequestLogger(mockBigBrother.Object);

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    requestBuilder,
                    requestLogger,
                    mockBigBrother.Object,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<Uri>(), _cancellationToken), Times.Once);
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

            var httpClients = new IndexDictionary<string, HttpClient>
            {
                {new Uri(config.WebhookConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.CallbackConfig.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new Mock<IAcquireTokenHandler>().Object);

            var httpClientBuilder = new HttpClientBuilder(mockAuthHandlerFactory.Object, httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new RequestBuilder(),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(),
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<string>(), _cancellationToken), Times.Never);
            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<Uri>(), _cancellationToken), Times.Once);
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

            var httpClients = new IndexDictionary<string, HttpClient>
            {
                {new Uri(config.CallbackConfig.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            //for each route in the path query, we create a mock http client in the factory
            foreach (var rules in config.WebhookConfig.WebhookRequestRules.Where(r => r.Routes.Any()))
            {
                foreach (var route in rules.Routes)
                {
                    httpClients.Add(new Uri(route.Uri).Host, mockHttpHandler.ToHttpClient());
                }
            }

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            var httpClientBuilder = new HttpClientBuilder(mockAuthHandlerFactory.Object, httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new RequestBuilder(),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(),
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<string>(), _cancellationToken), Times.AtMostOnce);
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
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new IndexDictionary<string, HttpClient>
            {
                {new Uri(config.WebhookConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.CallbackConfig.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            var httpClientBuilder = new HttpClientBuilder(mockAuthHandlerFactory.Object, httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new RequestBuilder(),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(),
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            await Assert.ThrowsAsync<Exception>(async () => await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken));
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
            WebhookConfig = new WebhookConfig
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
            WebhookConfig = new WebhookConfig
            {
                Name = "Webhook1",
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
            WebhookConfig = new WebhookConfig
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
