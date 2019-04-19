using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Eshopworld.Tests.Core;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Authentication
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
                Uri = "http://localhost/authendpoint"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, config.Uri)
                .WithHeaders("client_id", config.ClientId)
                .WithHeaders("client_secret", config.ClientSecret)
                .WithContentType("application/json-patch+json", string.Empty)
                .Respond(HttpStatusCode.Created, "application/json-patch+json", expectedResponse);

            var handler = new MmAuthenticationHandler(config, _bigBrother);
            var httpClient = mockHttp.ToHttpClient();
            await handler.GetTokenAsync(httpClient, _cancellationToken);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
