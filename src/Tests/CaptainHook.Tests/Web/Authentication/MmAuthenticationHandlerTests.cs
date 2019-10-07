using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Eshopworld.Tests.Core;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.Authentication
{
    public class MmAuthenticationHandlerTests
    {
        private readonly IBigBrother _bigBrother;
        private readonly CancellationToken _cancellationToken;

        public MmAuthenticationHandlerTests()
        {
            _bigBrother = new Mock<BigBrother>().Object;
            _cancellationToken = new CancellationToken();
        }

        [IsLayer0]
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task AuthorisationTokenSuccessTests(string expectedAccessToken)
        {
            var expectedResponse = JsonConvert.SerializeObject(new OidcAuthenticationToken
            {
                AccessToken = expectedAccessToken
            });

            var config = new OidcAuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Uri = "https://localhost/authendpoint"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, config.Uri)
                .WithHeaders("client_id", config.ClientId)
                .WithHeaders("client_secret", config.ClientSecret)
                .WithContentType("application/json-patch+json; charset=utf-8", string.Empty)
                .Respond(HttpStatusCode.Created, "application/json-patch+json", expectedResponse);

            var httpClientFactory = new HttpClientFactory(new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } });


            var handler = new MmAuthenticationHandler(httpClientFactory, config, _bigBrother);
            var httpClient = mockHttp.ToHttpClient();
            var token = await handler.GetTokenAsync(_cancellationToken);

            Assert.NotNull(token);
            Assert.NotEmpty(token);
            Assert.StartsWith("Bearer ", token);
        }
    }
}
