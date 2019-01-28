using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Tests.Core;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Authentication
{
    public class AuthenticationHandlerTests
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
                Scopes = new[] { "bob.scope.all" },
                Uri = "http://localhost/authendpoint"
            };

            var handler = new OAuthTokenHandler(config);

            var httpMessageHandler = EventHandlerTestHelper.GetMockHandler(new StringContent(expectedResponse));
            var httpClient = new HttpClient(httpMessageHandler.Object);
            await handler.GetToken(httpClient);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
