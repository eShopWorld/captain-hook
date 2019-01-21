using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class MmAuthenticationHandlerTests
    {
        [Theory]
        [InlineData("108923740981723857198237451982735jsdhfkjsdhfjasdf")]
        public async Task GetToken(string expectedToken)
        {
            var expectedResponse = JsonConvert.SerializeObject(new AuthToken
            {
                AccessToken = expectedToken
            });

            var mockHttpHandler = EventHandlerTestHelper.GetMockHandler(new StringContent(expectedResponse), HttpStatusCode.Created);
            var httpClient = new HttpClient(mockHttpHandler.Object);

            var config = new AuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Uri = "http://localhost/authendpoint"
            };

            var handler = new MmAuthenticationHandler(config);
            await handler.GetToken(httpClient);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
