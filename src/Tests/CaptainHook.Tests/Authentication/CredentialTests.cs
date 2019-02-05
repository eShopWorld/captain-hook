using System.Net.Http;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using IdentityModel.Client;
using Xunit;

namespace CaptainHook.Tests.Authentication
{
    public class CredentialTests
    {
        [Theory(Skip = "For debugging untilplugged into keyvault and into the correct flow")]
        [IsLayer1]
        [InlineData("esw.nike.snkrs.controltower.webhook.api.all")]
        [InlineData("esw.nike.snkrs.product.webhook.api.all")]
        [InlineData("checkout.webhook.api.all")]
        public async Task GetCredentials(string scope)
        {
            var client = new HttpClient();
            var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://security-sts.test.eshopworld.net/connect/token",
                ClientId = "tooling.eda.client",
                ClientSecret = "",
                GrantType = "client_credentials",
                Scope = scope
            });

            Assert.Equal(ResponseErrorType.None, response.ErrorType);
        }
    }
}