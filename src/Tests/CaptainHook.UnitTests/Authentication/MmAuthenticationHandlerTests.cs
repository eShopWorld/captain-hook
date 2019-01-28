using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Tests.Core;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class MmAuthenticationHandlerTests
    {
        [IsLayer0]
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task AuthorisationTokenSuccessTests(string expectedAccessToken)
        {
            var expectedResponse = JsonConvert.SerializeObject(new OAuthAuthenticationToken
            {
                AccessToken = expectedAccessToken
            });

            var config = new OAuthAuthenticationConfig
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

            var handler = new MmOAuthAuthenticationHandler(config);
            var httpClient = mockHttp.ToHttpClient();
            await handler.GetToken(httpClient);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
