using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using IdentityModel.Client;
using Xunit;

namespace CaptainHook.Tests.Authentication
{
    public class CredentialTests
    {
        [Theory(Skip = "Will enable when getting to layer 1 tests in V1")]
        [IsLayer1]
        [InlineData("esw.nike.snkrs.controltower.webhook.api.all")]
        [InlineData("esw.nike.snkrs.product.webhook.api.all")]
        [InlineData("checkout.webhook.api.all")]
        public async Task GetCredentials(string scope)
        {
            var client = new HttpClient();
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://security-sts.ci.eshopworld.net/connect/token",
                ClientId = "tooling.eda.client",
                ClientSecret = "goodluck",
                GrantType = "client_credentials",
                Scope = scope
            }, CancellationToken.None);

            Assert.Equal(ResponseErrorType.None, response.ErrorType);
        }
    }
}
