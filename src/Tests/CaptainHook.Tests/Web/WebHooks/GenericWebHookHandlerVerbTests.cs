using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
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
    /// <summary>
    /// Tests the HTTP HttpMethod selection maps to the actual requests made to the webhooks and callbacks
    /// </summary>
    public class GenericWebHookHandlerVerbTests
    {
        private readonly CancellationToken _cancellationToken;
        private readonly FeatureFlagsConfiguration _featureFlags;

        public GenericWebHookHandlerVerbTests()
        {
            _cancellationToken = new CancellationToken();
            _featureFlags = new FeatureFlagsConfiguration();
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(CreationData))]
        public async Task ChecksHttpCreationVerbs(WebhookConfig config, HttpMethod httpMethod, string payload, HttpStatusCode expectedResponseCode, string expectedResponseBody)
        {
            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(httpMethod, config.Uri)
                .WithContentType("application/json", payload)
                .Respond(expectedResponseCode, "application/json", expectedResponseBody);

            var mockBigBrother = new Mock<IBigBrother>();

            var host = new Uri(config.Uri).Host;
            var httpClients = new Dictionary<string, HttpClient> { { host, mockHttp.ToHttpClient() } };
            var mockAuthenticationFactory = new Mock<IAuthenticationHandlerFactory>();

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var httpSender = new HttpSender(httpClientBuilder);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _featureFlags);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpSender,
                mockAuthenticationFactory.Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            await genericWebhookHandler.CallAsync(new MessageData(payload, "TestType", "subA", "service") {ServiceBusMessageId = Guid.NewGuid().ToString(), CorrelationId = Guid.NewGuid().ToString()}, new Dictionary<string, object>(), _cancellationToken);
            Assert.Equal(1, mockHttp.GetMatchCount(request));
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task ChecksHttpGetVerb(WebhookConfig config, HttpMethod httpMethod, string payload, HttpStatusCode expectedResponseCode, string expectedResponseBody)
        {
            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(httpMethod, config.Uri)
                .Respond(expectedResponseCode, "application/json", expectedResponseBody);

            var mockBigBrother = new Mock<IBigBrother>();
            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var httpSender = new HttpSender(httpClientBuilder);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _featureFlags);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpSender,
                new Mock<IAuthenticationHandlerFactory>().Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            await genericWebhookHandler.CallAsync(new MessageData(payload, "TestType", "subA", "service") { CorrelationId = Guid.NewGuid().ToString(), ServiceBusMessageId = Guid.NewGuid().ToString()}, new Dictionary<string, object>(), _cancellationToken);
            Assert.Equal(1, mockHttp.GetMatchCount(request));
        }

        /// <summary>
        /// CreationData for the theory above
        /// </summary>
        public static IEnumerable<object[]> CreationData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/post", HttpMethod = HttpMethod.Post, EventType = "Event1"}, HttpMethod.Post, "{\"Message\":\"Hello World Post\"}", HttpStatusCode.Created, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/put", HttpMethod = HttpMethod.Put, EventType = "Event2"}, HttpMethod.Put, "{\"Message\":\"Hello World Put\"}", HttpStatusCode.NoContent, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/patch", HttpMethod = HttpMethod.Patch, EventType = "Event3"}, HttpMethod.Patch, "{\"Message\":\"Hello World Patch\"}", HttpStatusCode.NoContent, string.Empty  },
            };

        /// <summary>
        /// Get Data 
        /// </summary>
        public static IEnumerable<object[]> GetData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/get", HttpMethod = HttpMethod.Get, EventType = "Event1" }, HttpMethod.Get, null, HttpStatusCode.OK, string.Empty}
            };
    }
}
