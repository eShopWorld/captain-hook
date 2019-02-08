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
using Newtonsoft.Json;
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
        [MemberData(nameof(Data))]
        public async Task ExecuteHappyPath(EventHandlerConfig config, MessageData messageData)
        {
            var mockHttpHandler = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
            var mockWebHookRequestWithCallback = mockHttpHandler.When(HttpMethod.Put, config.WebHookConfig.Uri)
                .WithContentType("application/json", JsonConvert.SerializeObject(new { Name = "Hello World" }))
                .Respond(HttpStatusCode.OK, "application/json", "hello");

            var mockWebHookRequest = mockHttpHandler.When(HttpMethod.Post, config.CallbackConfig.Uri)
                .Respond(HttpStatusCode.OK, "application/json", "hello");

            var httpClient = mockHttpHandler.ToHttpClient();

            var mockAuthHandler = new Mock<IAcquireTokenHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            var mockHandlerFactory = new Mock<IEventHandlerFactory>();
            mockHandlerFactory.Setup(s => s.CreateWebhookHandler(config.CallbackConfig.Name)).Returns(
                new GenericWebhookHandler(
                    mockAuthHandler.Object,
                    mockBigBrother.Object,
                    httpClient,
                    config.CallbackConfig));

            var webhookResponseHandler = new WebhookResponseHandler(
                mockHandlerFactory.Object,
                mockAuthHandler.Object,
                mockBigBrother.Object,
                httpClient,
                config);

            await webhookResponseHandler.Call(messageData);

            mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.Exactly(2));
            mockHandlerFactory.Verify(e => e.CreateWebhookHandler(It.IsAny<string>()), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequestWithCallback));
            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequest));
        }


        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[]
                {
                    new EventHandlerConfig
                    {
                        Name = "Event 1",
                        Type = "blahblah",
                        WebHookConfig = new WebhookConfig
                        {
                            Name = "Webhook1",
                            HttpVerb = "POST",
                            Uri = "https://blah.blah.eshopworld.com",
                            AuthenticationConfig = new OidcAuthenticationConfig
                            {
                                Type = AuthenticationType.OIDC,
                                Uri = "https://blah-blah.sts.eshopworld.com",
                                ClientId = "ClientId",
                                ClientSecret = "ClientSecret",
                                Scopes = new[] {"scope1", "scope2"}
                            },
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Name = "OrderCode",
                                    Source = new ParserLocation
                                    {
                                        Location = Location.PayloadBody,
                                        Path = "OrderCode"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.Uri
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Type = QueryRuleTypes.WebHook,
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
                        CallbackConfig = new WebhookConfig
                        {
                            Name = "PutOrderConfirmationEvent",
                            HttpVerb = "PUT",
                            Uri = "https://callback.eshopworld.com",
                            AuthenticationConfig = new AuthenticationConfig
                            {
                                Type = AuthenticationType.None
                            },
                            WebhookRequestRules = new List<WebhookRequestRule>
                            {
                                new WebhookRequestRule
                                {
                                    Type = QueryRuleTypes.Parameter,
                                    Source = new ParserLocation
                                    {
                                        Location = Location.MessageBody,
                                        Path = "OrderCode"
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.PayloadBody,
                                        Path = "OrderCode"
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Type = QueryRuleTypes.Parameter,
                                    Source = new ParserLocation
                                    {
                                        Location = Location.StatusCode,
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.PayloadBody,
                                        Path = "StatusCode"
                                    }
                                },
                                new WebhookRequestRule
                                {
                                    Type = QueryRuleTypes.Parameter,
                                    Source = new ParserLocation
                                    {
                                        Location = Location.PayloadBody
                                    },
                                    Destination = new ParserLocation
                                    {
                                        Location = Location.PayloadBody,
                                        Path = "Content"
                                    }
                                }
                            }
                        }
                    },
                    new MessageData
                    {
                        Payload = EventHandlerTestHelper.GenerateMockPayloadWithInternalModel(Guid.NewGuid()),
                        Type = "TestType"
                    }
                }
            };
    }
}
