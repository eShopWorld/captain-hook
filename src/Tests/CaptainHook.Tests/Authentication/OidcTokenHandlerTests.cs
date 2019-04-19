using System;
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
    public class OidcTokenHandlerTests
    {
        private readonly IBigBrother _bigBrother;
        private readonly CancellationToken _cancellationToken;

        public OidcTokenHandlerTests()
        {
            _bigBrother = new Mock<BigBrother>().Object;
            _cancellationToken = new CancellationToken();
        }

        [IsLayer0]
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task AuthorisationTokenSuccess(string expectedAccessToken)
        {
            var config = new OidcAuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Scopes = new[] { "bob.scope.all" },
                Uri = "http://localhost/authendpoint"
            };
            var handler = new OidcAuthenticationHandler(config, _bigBrother);

            var mockHttp = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
            var mockRequest = mockHttp.When(HttpMethod.Post, config.Uri)
                .WithFormData("client_id", config.ClientId)
                .WithFormData("client_secret", config.ClientSecret)
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new OidcAuthenticationToken
                    {
                        AccessToken = "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282"
                    }));

            var httpClient = mockHttp.ToHttpClient();

            await handler.GetTokenAsync(httpClient, _cancellationToken);

            Assert.Equal(1, mockHttp.GetMatchCount(mockRequest));
            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }

        /// <summary>
        /// Tests the refresh of a token. 
        /// First call is to get a token. Second call is to get a token but through the refresh flow.
        /// Validated by the expected call count against the same STS URI. 
        /// </summary>
        /// <param name="refreshBeforeInSeconds"></param>
        /// <param name="expiryTimeInSeconds"></param>
        /// <param name="expectedStsCallCount"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [IsLayer0]
        [Theory]
        [InlineData(0, 5, 1, "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        [InlineData(1, 5, 1, "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        [InlineData(5, 5, 2, "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        [InlineData(5, 10, 1, "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        public async Task RefreshToken(int refreshBeforeInSeconds, int expiryTimeInSeconds, int expectedStsCallCount, string token)
        {
            var config = new OidcAuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Scopes = new[] { "bob.scope.all" },
                Uri = "http://localhost/authendpoint",
                RefreshBeforeInSeconds = refreshBeforeInSeconds
            };

            var handler = new OidcAuthenticationHandler(config, _bigBrother);

            var mockHttp = new MockHttpMessageHandler();
            var mockRequest = mockHttp.When(HttpMethod.Post, config.Uri)
                .WithFormData("client_id", config.ClientId)
                .WithFormData("client_secret", config.ClientSecret)
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new OidcAuthenticationToken
                    {
                        AccessToken = token,
                        ExpiresIn = expiryTimeInSeconds
                    }));

            var httpClient = mockHttp.ToHttpClient();

            await handler.GetTokenAsync(httpClient, _cancellationToken);

            await Task.Delay(TimeSpan.FromSeconds(1), _cancellationToken);

            await handler.GetTokenAsync(httpClient, _cancellationToken);

            Assert.Equal(expectedStsCallCount, mockHttp.GetMatchCount(mockRequest));
            Assert.Equal(token, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
