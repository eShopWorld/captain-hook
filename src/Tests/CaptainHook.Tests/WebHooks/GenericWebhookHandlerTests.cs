using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
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
using Moq.Protected;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.WebHooks
{
    public class GenericWebHookHandlerVerbTests
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<IAcquireTokenHandler> _mockAuthHandler;

        /// <summary>
        /// 
        /// </summary>
        public GenericWebHookHandlerVerbTests()
        {
            _mockAuthHandler = new Mock<IAcquireTokenHandler>();
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(Data))]
        public async Task ChecksHttpVerbsMatchUp(WebhookConfig config, HttpMethod httpMethod, string payload, HttpStatusCode expectedResponseCode, string expectedResponseBody)
        {
            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(httpMethod, config.Uri)
                .Respond(expectedResponseCode, "application/json", expectedResponseBody);

            if (payload != null)
            {
                request.WithContentType("application/json", payload);
            }

            var genericWebhookHandler = new GenericWebhookHandler(
                _mockAuthHandler.Object,
                new Mock<IBigBrother>().Object,
                mockHttp.ToHttpClient(),
                config);

            await genericWebhookHandler.Call(new MessageData { Payload = payload });
            Assert.Equal(1, mockHttp.GetMatchCount(request));
        }

        /// <summary>
        /// Data for the theory above
        /// </summary>
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/post", Verb = "Post", }, HttpMethod.Post, JsonConvert.SerializeObject(new { Message = "Hello World Post" }), HttpStatusCode.Created, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/put", Verb = "Put"}, HttpMethod.Put, JsonConvert.SerializeObject(new { Message = "Hello World Put " }), HttpStatusCode.NoContent, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/patch", Verb = "Patch"}, HttpMethod.Patch, JsonConvert.SerializeObject(new { Message = "Hello World Patch" }), HttpStatusCode.NoContent, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/get", Verb = "Get"}, HttpMethod.Get, null, HttpStatusCode.OK, string.Empty }
            };
    }

    public class GenericWebhookHandlerTests
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly GenericWebhookHandler _genericWebhookHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<IAcquireTokenHandler> _mockAuthHandler;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;

        public GenericWebhookHandlerTests()
        {
            _mockHttpHandler = EventHandlerTestHelper.GetMockHandler(new StringContent("hello"));
            var httpClient = new HttpClient(_mockHttpHandler.Object);
            var mockBigBrother = new Mock<IBigBrother>();
            _mockAuthHandler = new Mock<IAcquireTokenHandler>();

            _genericWebhookHandler = new GenericWebhookHandler(
                _mockAuthHandler.Object,
                mockBigBrother.Object,
                httpClient, new WebhookConfig
                {
                    Uri = "http://localhost/webhook",
                    ModelToParse = "TransportModel",
                    Verb = "PUT"
                });
        }

        [Fact]
        [IsLayer0]
        public async Task ExecuteHappyPath()
        {
            var messageData = new MessageData
            {
                Payload = EventHandlerTestHelper.GenerateMockPayloadWithInternalModel(Guid.NewGuid()),
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
