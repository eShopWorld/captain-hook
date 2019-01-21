using System;
using System.Collections.Generic;
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

namespace CaptainHook.UnitTests
{
    public class WebhookResponseHandlerTests
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly WebhookResponseHandler _webhookResponseHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<IAuthHandler> _mockAuthHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<IHandlerFactory> _mockHandlerFactory;

        public WebhookResponseHandlerTests()
        {
            _mockHttpHandler = EventHandlerTestHelper.GetMockHandler(new StringContent("hello"));
            _mockAuthHandler = new Mock<IAuthHandler>();
            var mockBigBrother = new Mock<IBigBrother>();

            _mockHandlerFactory = new Mock<IHandlerFactory>();
            _mockHandlerFactory.Setup(s => s.CreateHandler(It.IsAny<string>())).Returns(
                new GenericWebhookHandler(
                    _mockAuthHandler.Object,
                    mockBigBrother.Object,
                    new HttpClient(_mockHttpHandler.Object),
                    new WebhookConfig
                    {
                        Uri = "http://localhost/callback",
                        Name = "CallbackConfig"
                    }));

            _webhookResponseHandler = new WebhookResponseHandler(
                _mockHandlerFactory.Object,
                _mockAuthHandler.Object,
                mockBigBrother.Object,
                new HttpClient(_mockHttpHandler.Object),
                new EventHandlerConfig
                {
                    EventParsers = new EventParsers
                    {
                        Name = "TestType",
                        ModelQueryPath = "TransportModel"
                    },
                    WebHookConfig = new WebhookConfig
                    {
                        Uri = "http://localhost/webhook"
                    },
                    CallbackConfig = new WebhookConfig
                    {
                        Uri = "http://localhost/callback"
                    }
                });
        }

        [Fact]
        public async Task ExecuteHappyPath()
        {
            var messageData = new MessageData
            {
                Payload = EventHandlerTestHelper.GenerateMockPayloadWithInternalModel(Guid.NewGuid()),
                Type = "TestType"
            };

            await _webhookResponseHandler.Call(messageData);

            _mockAuthHandler.Verify(e => e.GetToken(It.IsAny<HttpClient>()), Times.Exactly(2));
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtMostOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == new Uri("http://localhost/webhook")),
                ItExpr.IsAny<CancellationToken>());

            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtMostOnce(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri == new Uri("http://localhost/callback")),
                ItExpr.IsAny<CancellationToken>());

            _mockHandlerFactory.Verify(e => e.CreateHandler(It.IsAny<string>()), Times.AtMostOnce);
        }
    }
}
