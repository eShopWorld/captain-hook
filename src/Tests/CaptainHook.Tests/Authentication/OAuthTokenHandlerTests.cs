using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Tests.Core;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Authentication
{
    public class OAuthTokenHandlerTests
    {
        [IsLayer0]
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task AuthorisationTokenSuccess(string expectedAccessToken)
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

        /// <summary>
        /// Tests the refresh of a token. 
        /// First call is to get a token. Second call is to get a token but through the refresh flow. 
        /// </summary>
        /// <param name="refreshBeforeInSeconds"></param>
        /// <param name="expectedStsCallCount"></param>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 1)]
        [InlineData(5, 2)]
        public async Task RefreshToken(int refreshBeforeInSeconds, int expectedStsCallCount)
        {
            var expectedResponse = JsonConvert.SerializeObject(new OAuthAuthenticationToken
            {
                AccessToken = "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282",
                ExpiresIn = 5
            });

            var handler = new OAuthTokenHandler(new OAuthAuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Scopes = new[] { "bob.scope.all" },
                Uri = "http://localhost/authendpoint",
                RefreshBeforeInSeconds = refreshBeforeInSeconds
            });

            var httpMessageHandler = EventHandlerTestHelper.GetMockHandler(new StringContent(expectedResponse));
            var httpClient = new HttpClient(httpMessageHandler.Object);

            await handler.GetToken(httpClient);

            await Task.Delay(TimeSpan.FromSeconds(1));

            await handler.GetToken(httpClient);

            httpMessageHandler.Protected().Verify(
                "SendAsync",
                Times.Exactly(expectedStsCallCount),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post && req.RequestUri == new Uri("http://localhost/authendpoint")),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
