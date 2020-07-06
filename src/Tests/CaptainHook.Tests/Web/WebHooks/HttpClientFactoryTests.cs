using System;
using System.Collections.Generic;
using System.Net.Http;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using Eshopworld.Tests.Core;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class HttpClientFactoryTests
    {
        [IsUnit]
        [Theory]
        [MemberData(nameof(Data))]
        public void CanGetHttpClient(WebhookConfig config)
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);

            var httpClient = httpClientBuilder.Get(config);

            Assert.NotNull(httpClient);
        }

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/post", HttpMethod = HttpMethod.Post }}
            };
    }
}