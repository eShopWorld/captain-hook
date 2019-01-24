using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class AuthenticationHandlerTests
    {
        [IsLayer1]
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task AuthorisationTokenSuccessTests(string expectedAccessToken)
        {
            var expectedResponse = JsonConvert.SerializeObject(new OAuthAuthenticationToken
            {
                AccessToken = expectedAccessToken
            });

            var config = new AuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Scopes = "bob.scope.all",
                Uri = "http://localhost/authendpoint"
            };

            var handler = new OAuthAuthenticationHandler(config, new Mock<IBigBrother>().Object);

            var httpMessageHandler = EventHandlerTestHelper.GetMockHandler(new StringContent(expectedResponse));
            var httpClient = new HttpClient(httpMessageHandler.Object);
            await handler.GetToken(httpClient);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
