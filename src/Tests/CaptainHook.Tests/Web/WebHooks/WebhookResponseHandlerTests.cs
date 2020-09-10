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
using Moq.Language.Flow;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class WebhookResponseHandlerTests
    {
        private const string ExpectedWebHookUri = "https://blah.blah.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
        private const string ExpectedCallbackUri = "https://callback.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
        private const string ExpectedContent = "{\"TransportModel\":\"{\\\"Name\\\":\\\"Hello World\\\"}\"}";
        private const string ResponseContentMsgHelloWorld = "{\"msg\":\"Hello World\"}";

        private readonly CancellationToken _cancellationToken;
        private readonly Mock<IHttpSender> _mockHttpSender;
        private readonly Mock<IBigBrother> _mockBigBrother;
        private readonly Mock<IAuthenticationHandlerFactory> _mockAuthHandlerFactory;
        private readonly StringContent _responseStringContent = new StringContent(ResponseContentMsgHelloWorld, System.Text.Encoding.UTF8, "application/json");

        public WebhookResponseHandlerTests()
        {
            _cancellationToken = new CancellationToken();
            _mockHttpSender = new Mock<IHttpSender>();
            _mockBigBrother = new Mock<IBigBrother>();
            _mockAuthHandlerFactory = CreateMockAuthHandlerFactory();
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

        private WebhookResponseHandler BuildWebhookResponseHandler(MessageData messageData,
            Mock<IAuthenticationHandlerFactory> mockAuthHandlerFactory)
        {
            var requestBuilder = new DefaultRequestBuilder(_mockBigBrother.Object);
            var requestLogger = new RequestLogger(_mockBigBrother.Object, new FeatureFlagsConfiguration());
            var handlerFactory = new EventHandlerFactory(_mockBigBrother.Object, _mockHttpSender.Object,
                mockAuthHandlerFactory.Object, requestLogger, requestBuilder);

            var webhookResponseHandler = new WebhookResponseHandler(
                handlerFactory,
                _mockHttpSender.Object,
                requestBuilder,
                mockAuthHandlerFactory.Object,
                requestLogger,
                _mockBigBrother.Object,
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

            SetupMockHttpSender(HttpMethod.Post, ExpectedWebHookUri, HttpStatusCode.OK, _responseStringContent);
            SetupMockHttpSender(HttpMethod.Put, ExpectedCallbackUri, HttpStatusCode.OK);

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, _mockAuthHandlerFactory);
            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            _mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(ExpectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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

            SetupMockHttpSender(HttpMethod.Post, ExpectedWebHookUri, HttpStatusCode.OK, _responseStringContent);
            SetupMockHttpSender(HttpMethod.Put, ExpectedCallbackUri, HttpStatusCode.OK, _responseStringContent);

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, _mockAuthHandlerFactory);
            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            _mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);
            _mockHttpSender.Verify(x => x.SendAsync(messageData.SubscriberConfig.HttpMethod, It.Is<Uri>(uri => uri.AbsoluteUri.StartsWith(messageData.SubscriberConfig.Uri)), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockHttpSender.Verify(x => x.SendAsync(messageData.SubscriberConfig.Callback.HttpMethod, It.Is<Uri>(uri => uri.AbsoluteUri.StartsWith(messageData.SubscriberConfig.Callback.Uri)), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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

            SetupMockHttpSender(HttpMethod.Post, ExpectedWebHookUri, HttpStatusCode.OK, _responseStringContent).Verifiable();
            SetupMockHttpSender(HttpMethod.Put, ExpectedCallbackUri, HttpStatusCode.ServiceUnavailable, _responseStringContent).Verifiable();

            var mockAuthHandlerFactory = _mockAuthHandlerFactory;
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockAuthHandlerFactory);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);

            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Put, new Uri(ExpectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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

            SetupMockHttpSender(HttpMethod.Post, ExpectedWebHookUri, HttpStatusCode.ServiceUnavailable);
            SetupMockHttpSender(HttpMethod.Put, ExpectedCallbackUri, HttpStatusCode.OK);

            var mockAuthHandlerFactory = _mockAuthHandlerFactory;
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockAuthHandlerFactory);

            var messageDelivery = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.Once);

            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(ExpectedWebHookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Put, new Uri(ExpectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Never);

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
            const string expectedMultiWebhookUri = "https://blah.blah.multiroute.eshopworld.com/BB39357A-90E1-4B6A-9C94-14BD1A62465E";
            
            var messageData = CreateMessageDataPayload();
            messageData.SubscriberConfig = EventHandlerConfigWithGoodMultiRoute;

            SetupMockHttpSender(HttpMethod.Post, expectedMultiWebhookUri, HttpStatusCode.OK, _responseStringContent);
            SetupMockHttpSender(messageData.SubscriberConfig.Callback.HttpMethod, ExpectedCallbackUri, HttpStatusCode.OK, _responseStringContent);

            var mockAuthHandlerFactory = _mockAuthHandlerFactory;
            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockAuthHandlerFactory);

            await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            mockAuthHandlerFactory.Verify(e => e.GetAsync(It.IsAny<WebhookConfig>(), _cancellationToken), Times.AtMostOnce);
            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(expectedMultiWebhookUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockHttpSender.Verify(x => x.SendAsync(HttpMethod.Post, new Uri(ExpectedCallbackUri), It.IsAny<WebHookHeaders>(), It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()), Times.Once);
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

            var mockAuthHandlerFactory = _mockAuthHandlerFactory;

            _mockBigBrother.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "route mapping/selector not found between config and the properties on the domain object"), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            SetupMockHttpSender(HttpMethod.Post, messageData.SubscriberConfig.Uri, HttpStatusCode.OK);
            SetupMockHttpSender(HttpMethod.Post, messageData.SubscriberConfig.Callback.Uri, HttpStatusCode.OK);

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockAuthHandlerFactory);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.True(result);
            _mockBigBrother.VerifyAll();
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
            mockHttpHandler.When(HttpMethod.Post, ExpectedWebHookUri)
                .WithContentType("application/json; charset=utf-8", ExpectedContent)
                .Respond(HttpStatusCode.OK, "application/json", ResponseContentMsgHelloWorld);

            var httpClients = new Dictionary<string, HttpClient>
            {
                {new Uri(messageData.SubscriberConfig.Uri).Host, mockHttpHandler.ToHttpClient()},
                {new Uri(messageData.SubscriberConfig.Callback.Uri).Host, mockHttpHandler.ToHttpClient()}
            };

            var mockAuthHandlerFactory = _mockAuthHandlerFactory;

            _mockBigBrother.Setup(b => b.Publish(It.Is<UnroutableMessageEvent>(e => e.Message == "routing path value in message payload is null or empty"),
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()));

            SetupMockHttpSender(HttpMethod.Post, messageData.SubscriberConfig.Uri, HttpStatusCode.OK);
            SetupMockHttpSender(HttpMethod.Post, messageData.SubscriberConfig.Callback.Uri, HttpStatusCode.OK);

            var webhookResponseHandler = BuildWebhookResponseHandler(messageData, mockAuthHandlerFactory);

            var result = await webhookResponseHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.True(result);
            _mockBigBrother.VerifyAll();
        }

        private IReturnsResult<IHttpSender> SetupMockHttpSender(HttpMethod httpMethod, string uri, HttpStatusCode responseStatusCode, StringContent responseContent = null)
        {
            var response = new HttpResponseMessage(responseStatusCode);
            if (responseContent != null)
                response.Content = responseContent;

            return _mockHttpSender
                .Setup(x => x.SendAsync(httpMethod, new Uri(uri), It.IsAny<WebHookHeaders>(),
                    It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(response);
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
