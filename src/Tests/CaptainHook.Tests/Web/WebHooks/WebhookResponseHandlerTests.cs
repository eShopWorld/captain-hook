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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Telemetry.Message;
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
        public async Task CheckWebhookCall(SubscriberConfiguration config, MessageData messageData, string expectedUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var mockWebHookRequestWithCallback = mockHttpHandler.When(HttpMethod.Post, expectedUri)
                .WithContentType("application/json", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var mockBigBrother = new Mock<IBigBrother>();

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockTokenHandler = new Mock<IAuthenticationHandler>();
            mockTokenHandler.Setup(s => s.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString);

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<WebhookConfig>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => mockTokenHandler.Object);

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var requestBuilder = new RequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object);

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    mockAuthHandlerFactory.Object,
                    requestBuilder,
                    requestLogger,
                    mockBigBrother.Object,
                    config.Callback));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                requestBuilder,
                mockAuthHandlerFactory.Object,
                requestLogger,
                mockBigBrother.Object,
                config);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequestWithCallback));
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(CallbackCallData))]
        public async Task CheckCallbackCall(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedCallbackUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var callbackRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthenticationTokenHandler = new Mock<IAuthenticationHandler>();
            mockAuthenticationTokenHandler.Setup(s => s.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid().ToString);

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<WebhookConfig>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => mockAuthenticationTokenHandler.Object);

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(Mock.Of<IBigBrother>()),
                mockAuthHandlerFactory.Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig)), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(callbackRequest));
            Assert.True(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback, with callback failing and message considered "not delivered"
        /// </summary>
        /// <returns>task</returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(CallbackCallData))]
        public async Task CheckCallbackFailurePath(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedCallbackUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var callbackRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
                .Respond(HttpStatusCode.ServiceUnavailable, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthenticationTokenHandler = new Mock<IAuthenticationHandler>();
            mockAuthenticationTokenHandler.Setup(s => s.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid().ToString);

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<WebhookConfig>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => mockAuthenticationTokenHandler.Object);

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(Mock.Of<IBigBrother>()),
                mockAuthHandlerFactory.Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig)), Times.AtMostOnce);

            Assert.Equal(3, mockHttpHandler.GetMatchCount(callbackRequest)); //current polly policy

            Assert.False(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback, with hook failing and message considered "not delivered" and callback not invoked
        /// </summary>
        /// <returns>task</returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(CallbackCallData))]
        public async Task CheckHookFailureDoesNotInvokeCallback(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedCallbackUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.ServiceUnavailable, "application/json", "{\"msg\":\"Hello World\"}");

            var callbackRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthenticationTokenHandler = new Mock<IAuthenticationHandler>();
            mockAuthenticationTokenHandler.Setup(s => s.GetTokenAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Guid.NewGuid().ToString);

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<WebhookConfig>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => mockAuthenticationTokenHandler.Object);

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(Mock.Of<IBigBrother>()),
                mockAuthHandlerFactory.Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig)), Times.AtMostOnce);

            Assert.Equal(0, mockHttpHandler.GetMatchCount(callbackRequest));

            Assert.False(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(GoodMultiRouteCallData))]
        public async Task GoodCheckMultiRouteSelection(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            var multiRouteCall = mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            //for each route in the path query, we create a mock http client in the factory
            foreach (var rules in config.WebhookRequestRules.Where(r => r.Routes.Any()))
            {
                foreach (var route in rules.Routes)
                {
                    httpClients.Add(new Uri(route.Uri).Host, mockHttpHandler.ToHttpClient());
                }
            }

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(Mock.Of<IBigBrother>()),
                new Mock<IAuthenticationHandlerFactory>().Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.AtMostOnce);
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig)), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(multiRouteCall));
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(BadMultiRouteCallData))]
        public async Task BadCheckMultiRouteSelection(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var bbMock = new Mock<IBigBrother>();
            bbMock.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "route mapping/selector not found between config and the properties on the domain object"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(bbMock.Object),
                new Mock<IAuthenticationHandlerFactory>().Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);
            Assert.True(result);
            bbMock.VerifyAll();
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(BadMultiRouteCallDataNoSelector))]
        public async Task BadCheckMultiRouteSelectionNoSelector(SubscriberConfiguration config, MessageData messageData, string expectedWebHookUri, string expectedContent)
        {
            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(config.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(config.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(It.Is<WebhookConfig>(x => x == messageData.WebhookConfig))).Returns(
                new GenericWebhookHandler(
                    httpClientBuilder,
                    new Mock<IAuthenticationHandlerFactory>().Object,
                    new RequestBuilder(Mock.Of<IBigBrother>()),
                    new RequestLogger(mockBigBrother.Object),
                    mockBigBrother.Object,
                    config.Callback));

            var bbMock = new Mock<IBigBrother>();
            bbMock.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "routing path value in message payload is null or empty"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                httpClientBuilder,
                new RequestBuilder(bbMock.Object),
                new Mock<IAuthenticationHandlerFactory>().Object,
                new RequestLogger(mockBigBrother.Object),
                mockBigBrother.Object,
                config);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);
            Assert.True(result);
            bbMock.VerifyAll();
        }

        public static IEnumerable<object[]> WebHookCallData =>
            new List<object[]>
            {
                new object[]
                {
                    SubscriberConfigurationWithSingleRoute,
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
                    SubscriberConfigurationWithSingleRoute,
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

        public static IEnumerable<object[]> BadMultiRouteCallDataNoSelector =>
            new List<object[]>
            {
                new object[]
                {
                    EventHandlerConfigWithBadMultiRoute,
                    EventHandlerTestHelper.CreateMessageDataPayload("").data,
                    "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E",
                    "{\"TransportModel\":{\"Name\":\"Hello World\"}}"
                }
            };

        private static SubscriberConfiguration SubscriberConfigurationWithSingleRoute => new SubscriberConfiguration
        {
            Name = "Webhook1",
            HttpMethod = HttpMethod.Post,
            Uri = "https://blah.blah.eshopworld.com",
            EventType = "Event1Webhook",
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
                },
            Callback = new WebhookConfig
            {
                Name = "PutOrderConfirmationEvent",
                HttpMethod = HttpMethod.Put,
                Uri = "https://callback.eshopworld.com",
                EventType = "Event1Callback",
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

        private static SubscriberConfiguration EventHandlerConfigWithGoodMultiRoute => new SubscriberConfiguration
        {
            Name = "Webhook1",
            EventType = "Event1Webhook",
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
                                HttpMethod = HttpMethod.Post,
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
                },
            Callback = new WebhookConfig
            {
                Name = "PutOrderConfirmationEvent",
                EventType = "PutOrderConfirmationEvent",
                HttpMethod = HttpMethod.Post,
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

        private static SubscriberConfiguration EventHandlerConfigWithBadMultiRoute => new EventHandlerConfig
        {
            Name = "Event 1",
            Type = "blahblah",
            WebhookConfig = new WebhookConfig
            {
                Name = "Webhook1",
                HttpMethod = HttpMethod.Post,
                EventType = "Event1",
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
                                HttpMethod = HttpMethod.Post,
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
                HttpMethod = HttpMethod.Post,
                Uri = "https://callback.eshopworld.com",
                EventType = "Event1Callback",
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
        }.AllSubscribers.First();
    }
}
