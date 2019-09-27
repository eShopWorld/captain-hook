using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.Tests.Web.Authentication;
using Eshopworld.Tests.Core;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class HttpClientBuilderTests
    {
        [IsLayer0]
        [Theory(Skip = "skipping for experimentation")]
        [MemberData(nameof(Data))]
        public async Task HttpClientContainsCorrelationIdHeader(WebhookConfig config, string expectedCorrelationId)
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClients = new IndexDictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientBuilder(new Mock<IAuthenticationHandlerFactory>().Object, httpClients);

            var httpClient = await httpClientBuilder.BuildAsync(config, AuthenticationType.None, expectedCorrelationId, CancellationToken.None);

            Assert.True(httpClient.DefaultRequestHeaders.Contains(Constants.Headers.CorrelationId));
        }

        [IsLayer0]
        [Theory(Skip = "skipping for experimentation")]
        [MemberData(nameof(Data))]
        public async Task CheckCorrelationIdHeadersMatch(WebhookConfig config, string expectedCorrelationId)
        {
            var mockHttp = new MockHttpMessageHandler();

            var httpClients = new IndexDictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientBuilder(new Mock<IAuthenticationHandlerFactory>().Object, httpClients);

            var httpClient = await httpClientBuilder.BuildAsync(config, AuthenticationType.None, expectedCorrelationId, CancellationToken.None);

            httpClient.DefaultRequestHeaders.TryGetValues(Constants.Headers.CorrelationId, out var value);
            Assert.Equal(expectedCorrelationId, value.First());
        }

        [IsLayer0]
        [Theory]
        [MemberData(nameof(Data))]
        public async Task EnsureHeadersAreAddedJustOnce(WebhookConfig config, string expectedCorrelationId)
        {
            var mockHttp = new MockHttpMessageHandler();
            var httpClient = mockHttp.ToHttpClient();
            httpClient.DefaultRequestHeaders.Add(Constants.Headers.CorrelationId, expectedCorrelationId);

            var httpClients = new IndexDictionary<string, HttpClient> { { new Uri(config.Uri).Host, httpClient } };

            var httpClientBuilder = new HttpClientBuilder(new Mock<IAuthenticationHandlerFactory>().Object, httpClients);
            var builtHttpClient = await httpClientBuilder.BuildAsync(config, AuthenticationType.None, expectedCorrelationId, CancellationToken.None);

            builtHttpClient.DefaultRequestHeaders.TryGetValues(Constants.Headers.CorrelationId, out var value);
            Assert.Single(value);
        }


        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Uri = "http://localhost/webhook/post", HttpVerb = HttpVerb.Post }, Guid.NewGuid().ToString()}
            };
    }
}