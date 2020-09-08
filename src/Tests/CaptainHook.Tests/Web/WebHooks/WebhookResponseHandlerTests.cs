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
using CaptainHook.Common.Telemetry.Message;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Requests;
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
        private const string expectedWebHookUri = "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
        private const string expectedCallbackUri = "https://callback.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
        private const string expectedContent = "{\"TransportModel\":\"{\\\"Name\\\":\\\"Hello World\\\"}\"}";

        private readonly CancellationToken _cancellationToken;

        public WebhookResponseHandlerTests()
        {
            _cancellationToken = new CancellationToken();
        }

        private static Mock<IAuthenticationHandlerFactory> CreateMockAuthHandlerFactory()
        {
            var mockTokenHandler = new Mock<IAuthenticationHandler>();
            mockTokenHandler.Setup(s => s.GetTokenAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid().ToString);

            var mockAuthHandlerFactory = new Mock<IAuthenticationHandlerFactory>();
            mockAuthHandlerFactory.Setup(s => s.GetAsync(It.IsAny<WebhookConfig>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => mockTokenHandler.Object);

            return mockAuthHandlerFactory;
        }

        private static WebhookResponseHandler BuildWebhookResponseHandler(MessageData messageData,
            Mock<IHttpSender> httpSender,
            Mock<IBigBrother> mockBigBrother,
            Mock<IAuthenticationHandlerFactory> mockAuthHandlerFactory)
        {
            var requestBuilder = new DefaultRequestBuilder(mockBigBrother.Object);
            var requestLogger = new RequestLogger(mockBigBrother.Object, new FeatureFlagsConfiguration());
            var handlerFactory = new EventHandlerFactory(mockBigBrother.Object, httpSender.Object,
                mockAuthHandlerFactory.Object, requestLogger, requestBuilder);

            var webhookResponseHandler = new WebhookResponseHandler(
                handlerFactory,
                httpSender.Object,
                requestBuilder,
                mockAuthHandlerFactory.Object,
                requestLogger,
                mockBigBrother.Object,
                messageData.SubscriberConfig);
            return webhookResponseHandler;
        }

        private static MessageData CreateMessageDataPayload(string brand = "Good")
        {
            var dictionary = new Dictionary<string, object>
            {
                {"OrderCode", "BB39357A-90E1-4B6A-9C94-14BD1A62465E"},
                {"BrandType", brand},
                {"TransportModel", new { Name = "Hello World" }}
            };

            var messageData = new MessageData(dictionary.ToJson(), "TestType", "subA", "service")
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceBusMessageId = Guid.NewGuid().ToString()
            };

            return messageData;
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsUnit]
        [Fact]
        public async Task CheckWebhookCall()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = SubscriberConfigurationWithSingleRoute;

            //string expectedContent = "{\"TransportModel\":\"{\\\"Name\\\":\\\"Hello World\\\"}\"}";

            //var mockHttpHandler = new MockHttpMessageHandler();
            //var mockWebHookRequestWithCallback = mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
            //    .WithContentType("application/json", expectedContent)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var httpClients = new Dictionary<string, HttpClient>
            //{
            //    {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
            //    {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            //};

            var mockHttpSender = new Mock<IHttpSender>();
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{\"msg\":\"Hello World\"}", System.Text.Encoding.UTF8, "application/json")
                });
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Put, new Uri(expectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsUnit]
        [Fact]
        public async Task CheckCallbackCall()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = SubscriberConfigurationWithSingleRoute;

            //var mockHttpHandler = new MockHttpMessageHandler();
            //mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
            //    .WithContentType("application/json; charset=utf-8", expectedContent)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var callbackRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var httpClients = new Dictionary<string, HttpClient>
            //{
            //    {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
            //    {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            //};

            var mockHttpSender = new Mock<IHttpSender>();
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Callback.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Callback.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.True(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback, with callback failing and message considered "not delivered"
        /// </summary>
        /// <returns>task</returns>
        [IsUnit]
        [Fact]
        public async Task CheckCallbackFailurePath()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = SubscriberConfigurationWithSingleRoute;

            var httpSender = new Mock<IHttpSender>();
            httpSender
                .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK))
                .Verifiable();
            httpSender
                .Setup(x => x.SendAsync(HttpMethod.Put, new Uri(expectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .Verifiable();

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, httpSender, mockBigBrother, mockAuthHandlerFactory);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);

            httpSender.Verify(x => x.SendAsync(HttpMethod.Put, new Uri(expectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            Assert.False(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback, with hook failing and message considered "not delivered" and callback not invoked
        /// </summary>
        /// <returns>task</returns>
        [IsUnit]
        [Fact]
        public async Task CheckHookFailureDoesNotInvokeCallback()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = SubscriberConfigurationWithSingleRoute;

            //var mockHttpHandler = new MockHttpMessageHandler();
            //mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
            //    .WithContentType("application/json; charset=utf-8", expectedContent)
            //    .Respond(HttpStatusCode.ServiceUnavailable, "application/json", "{\"msg\":\"Hello World\"}");

            //var callbackRequest = mockHttpHandler.When(HttpMethod.Put, expectedCallbackUri)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var httpClients = new Dictionary<string, HttpClient>
            //{
            //    {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
            //    {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            //};

            var mockHttpSender = new Mock<IHttpSender>();
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            mockHttpSender
                .Setup(x => x.SendAsync(HttpMethod.Put, new Uri(expectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);

            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Put, new Uri(expectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);

            Assert.False(messageDelivery);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsUnit]
        [Fact]
        public async Task GoodCheckMultiRouteSelection()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = EventHandlerConfigWithGoodMultiRoute;

            string expectedWebHookUri = "https://blah.blah.multiroute.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
            string expectedContent = "{\"TransportModel\":{\"Name\":\"Hello World\"}}";

            //var mockHttpHandler = new MockHttpMessageHandler();
            //var multiRouteCall = mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
            //    .WithContentType("application/json; charset=utf-8", expectedContent)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var httpClients = new Dictionary<string, HttpClient>
            //{
            //    {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            //};

            //for each route in the path query, we create a mock http client in the factory
            //var webhookConfigRoutes = messageData.SubscriberConfig.WebhookRequestRules.SelectMany(r => r.Routes);
            //foreach (var route in webhookConfigRoutes)
            //{
            //    httpClients.Add(new Uri(route.Uri).Host, mockHttpHandler.ToHttpClient());
            //}

            var mockHttpSender = new Mock<IHttpSender>();

            var webhookConfigRoutes = messageData.SubscriberConfig.WebhookRequestRules.SelectMany(r => r.Routes);
            foreach (var route in webhookConfigRoutes)
            {
                mockHttpSender
                    .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(route.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            }

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.AtMostOnce);
            mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Put, new Uri(expectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsUnit]
        [Fact]
        public async Task BadCheckMultiRouteSelection()
        {
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = EventHandlerConfigWithBadMultiRoute;

            //var mockHttpHandler = new MockHttpMessageHandler();
            //mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
            //    .WithContentType("application/json; charset=utf-8", expectedContent)
            //    .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            //var httpClients = new Dictionary<string, HttpClient>
            //{
            //    {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
            //    {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            //};

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();

            mockBigBrother.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "route mapping/selector not found between config and the properties on the domain object"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));
            
            var mockHttpSender = new Mock<IHttpSender>();
            mockHttpSender
                    .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            mockHttpSender
                    .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Callback.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.True(result);
            mockBigBrother.VerifyAll();
        }

        /// <summary>
        /// Tests the whole flow for a webhook handler with a callback
        /// </summary>
        /// <returns></returns>
        [IsUnit]
        [Fact]
        public async Task BadCheckMultiRouteSelectionNoSelector()
        {
            var messageData = EventHandlerTestHelper.CreateMessageDataPayload("").data;
            messageData.SubscriberConfig = EventHandlerConfigWithBadMultiRoute;

            var mockHttpHandler = new MockHttpMessageHandler();
            mockHttpHandler.When(HttpMethod.Post, expectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", expectedContent)
                .Respond(HttpStatusCode.OK, "application/json", "{\"msg\":\"Hello World\"}");

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockBigBrother = new Mock<IBigBrother>();
            var mockAuthHandlerFactory = CreateMockAuthHandlerFactory();

            mockBigBrother.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "routing path value in message payload is null or empty"),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            var mockHttpSender = new Mock<IHttpSender>();
            mockHttpSender
                    .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            mockHttpSender
                    .Setup(x => x.SendAsync(HttpMethod.Post, new Uri(messageData.SubscriberConfig.Callback.Uri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockHttpSender, mockBigBrother, mockAuthHandlerFactory);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.True(result);
            mockBigBrother.VerifyAll();
        }

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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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
