using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Moq;
using Moq.Protected;
using Xunit;

namespace CaptainHook.UnitTests.WebHooks
{
    public class GenericWebhookHandlerTests
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly GenericWebhookHandler _genericWebhookHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<IAuthHandler> _mockAuthHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;

        public GenericWebhookHandlerTests()
        {
            _mockHttpHandler = EventHandlerTestHelper.GetMockHandler(new StringContent("hello"));
            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var mockBigBrother = new Mock<IBigBrother>();
            _mockAuthHandler = new Mock<IAuthHandler>();

            _genericWebhookHandler = new GenericWebhookHandler(
                _mockAuthHandler.Object,
                mockBigBrother.Object,
                httpClient, new WebhookConfig
                {
                    Uri = "http://localhost/webhook"
                });
        }

        [Fact]
        public async Task ExecuteHappyPath()
        {
            var messageData = new MessageData
            {
                Payload = EventHandlerTestHelper.GenerateMockPayload(Guid.NewGuid()),
                Type = "TestType",
            };

            await _genericWebhookHandler.Call(messageData);

            _mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.AtMostOnce);
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtMostOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == new Uri("http://localhost/webhook")),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}
