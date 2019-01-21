using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class MmAuthenticationHandlerTests
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly MmAuthenticationHandler _handler;

        /// <summary>
        /// 
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        private readonly Mock<HttpMessageHandler> _mockHttpHandler;

        /// <summary>
        /// 
        /// </summary>
        private static string _expectedAccessToken = "6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282";

        /// <summary>
        /// Setup for all tests contained within
        /// </summary>
        public MmAuthenticationHandlerTests()
        {
            var expectedResponse = JsonConvert.SerializeObject(new AuthToken
            {
                AccessToken = _expectedAccessToken
            });

            _mockHttpHandler = EventHandlerTestHelper.GetMockHandler(new StringContent(expectedResponse), HttpStatusCode.Created);
            _httpClient = new HttpClient(_mockHttpHandler.Object);

            var config = new AuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Uri = "http://localhost/authendpoint"
            };

            _handler = new MmAuthenticationHandler(config);        }

        [Fact]
        public async Task AuthorisationTokenTests()
        {
            await _handler.GetToken(_httpClient);
            
            Assert.NotNull(_httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(_expectedAccessToken, _httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", _httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }

        [Fact]
        public void RequestTokenTests()
        {
            //verifies that the request was sent as a post and has a specific content type
            _mockHttpHandler.Protected().Verify(
                "SendAsync",
                Times.AtMostOnce(),
                ItExpr.Is<HttpRequestMessage>(
                    req => req.Method == HttpMethod.Post &&
                           req.RequestUri == new Uri("http://localhost/authendpoint") && req.Content.Headers.ContentType.MediaType == "application/json-patch+json"),
                ItExpr.IsAny<CancellationToken>());
        }
    }
}
