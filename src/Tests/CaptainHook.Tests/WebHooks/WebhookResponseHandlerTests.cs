using System;
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
        [Fact]
        public async Task ExecuteHappyPath()
        {
            var config = new EventHandlerConfig
            {
                WebHookConfig = new WebhookConfig
                {
                    Uri = "http://localhost/webhook",
                    ModelToParse = "TransportModel",
                    AuthenticationConfig = new OidcAuthenticationConfig(),
                    HttpVerb = "PUT"
                },
                CallbackConfig = new WebhookConfig
                {
                    Name = "PutOrderConfirmationEvent",
                    Uri = "http://localhost/callback",
                    AuthenticationConfig = new OidcAuthenticationConfig(),
                    HttpVerb = "POST"
                }
            };

            var messageData = new MessageData
            {
                Payload = EventHandlerTestHelper.GenerateMockPayloadWithInternalModel(Guid.NewGuid()),
                Type = "TestType"
            };

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
            mockHandlerFactory.Setup(s => s.CreateHandler(config.CallbackConfig.Name)).Returns(
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
            mockHandlerFactory.Verify(e => e.CreateHandler(It.IsAny<string>()), Times.AtMostOnce);

            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequestWithCallback));
            Assert.Equal(1, mockHttpHandler.GetMatchCount(mockWebHookRequest));
        }
    }
}
