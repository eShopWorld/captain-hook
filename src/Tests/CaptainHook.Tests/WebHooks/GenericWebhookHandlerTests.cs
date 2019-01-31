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
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.WebHooks
{
    public class GenericWebhookHandlerTests
    {
        [Fact]
        [IsLayer0]
        public async Task ExecuteHappyPath()
        {
            var messageData = new MessageData
            {
                Payload = EventHandlerTestHelper.GenerateMockPayloadWithInternalModel(Guid.NewGuid()),
                Type = "TestType",
            };

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                Verb = "PUT",
                AuthenticationConfig = new AuthenticationConfig()
            };

            var mockHttp = new MockHttpMessageHandler();
            var webhookRequest = mockHttp.When(HttpMethod.Put, config.Uri)
                .WithContentType("application/json", messageData.Payload)
                .Respond(HttpStatusCode.OK, "application/json", string.Empty);
            
            var genericWebhookHandler = new GenericWebhookHandler(
                new Mock<IAcquireTokenHandler>().Object,
                new Mock<IBigBrother>().Object,
                mockHttp.ToHttpClient(),
                config);

            await genericWebhookHandler.Call(messageData);
            
            Assert.Equal(1, mockHttp.GetMatchCount(webhookRequest));
        }
    }
}
