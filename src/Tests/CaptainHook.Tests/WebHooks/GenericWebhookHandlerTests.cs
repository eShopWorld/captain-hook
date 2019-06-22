using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly CancellationToken _cancellationToken;

        public GenericWebhookHandlerTests()
        {
            _cancellationToken = new CancellationToken();
        }

        [IsLayer0]
        [Fact]
        public async Task ExecuteHappyPath()
        {
            var (messageData, metaData) = EventHandlerTestHelper.CreateMessageDataPayload();

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                HttpVerb = HttpVerb.Put,
                AuthenticationConfig = new AuthenticationConfig(),
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
            };

            var mockHttp = new MockHttpMessageHandler();
            var webhookRequest = mockHttp.When(HttpMethod.Put, $"{config.Uri}/{metaData["OrderCode"]}")
                .WithContentType("application/json", messageData.Payload)
                .Respond(HttpStatusCode.OK, "application/json", string.Empty);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(s => s.CreateClient(It.IsAny<string>())).Returns(
                mockHttp.ToHttpClient());

            var genericWebhookHandler = new GenericWebhookHandler(
                new Mock<IAuthenticationHandlerFactory>().Object,
                new RequestBuilder(),
                new Mock<IBigBrother>().Object,
                httpClientFactory.Object,
                config);

            await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.Equal(1, mockHttp.GetMatchCount(webhookRequest));
        }
    }
}
