using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
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
    /// <summary>
    /// Tests the HTTP Verb selection maps to the actual requests made to the webhooks and callbacks
    /// </summary>
    public class GenericWebHookHandlerVerbTests
    {
        [IsLayer0]
        [Theory]
        [MemberData(nameof(CreationData))]
        public async Task ChecksHttpCreationVerbs(WebhookConfig config, HttpMethod httpMethod, string payload, HttpStatusCode expectedResponseCode, string expectedResponseBody)
        {
            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(httpMethod, config.Uri)
                .WithContentType("application/json", payload)
                .Respond(expectedResponseCode, "application/json", expectedResponseBody);

            var genericWebhookHandler = new GenericWebhookHandler(
                new Mock<IAcquireTokenHandler>().Object,
                new Mock<IBigBrother>().Object,
                mockHttp.ToHttpClient(),
                config);

            await genericWebhookHandler.Call(new MessageData { Payload = payload });
            Assert.Equal(1, mockHttp.GetMatchCount(request));
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(GetData))]
        public async Task ChecksHttpGetVerb(WebhookConfig config, HttpMethod httpMethod, string payload, HttpStatusCode expectedResponseCode, string expectedResponseBody)
        {
            var mockHttp = new MockHttpMessageHandler();
            var request = mockHttp.When(httpMethod, config.Uri)
                .Respond(expectedResponseCode, "application/json", expectedResponseBody);

            var genericWebhookHandler = new GenericWebhookHandler(
                new Mock<IAcquireTokenHandler>().Object,
                new Mock<IBigBrother>().Object,
                mockHttp.ToHttpClient(),
                config);

            await genericWebhookHandler.Call(new MessageData { Payload = payload });
            Assert.Equal(1, mockHttp.GetMatchCount(request));
        }

        /// <summary>
        /// CreationData for the theory above
        /// </summary>
        public static IEnumerable<object[]> CreationData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/post", Verb = "Post", }, HttpMethod.Post, JsonConvert.SerializeObject(new { Message = "Hello World Post" }), HttpStatusCode.Created, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/put", Verb = "Put"}, HttpMethod.Put, JsonConvert.SerializeObject(new { Message = "Hello World Put " }), HttpStatusCode.NoContent, string.Empty  },
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/patch", Verb = "Patch"}, HttpMethod.Patch, JsonConvert.SerializeObject(new { Message = "Hello World Patch" }), HttpStatusCode.NoContent, string.Empty  },
            };

        /// <summary>
        /// Get Data 
        /// </summary>
        public static IEnumerable<object[]> GetData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/get", Verb = "Get"}, HttpMethod.Get, null, HttpStatusCode.OK, string.Empty}
            };
    }
}