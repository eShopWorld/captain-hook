using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Custom Authentication Handler
    /// </summary>
    public class MmOAuthAuthenticationHandler : OAuthTokenHandler
    {
        public MmOAuthAuthenticationHandler(AuthenticationConfig authenticationConfig) 
            : base(authenticationConfig)
        {

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            if (string.IsNullOrEmpty(OAuthAuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(OAuthAuthenticationConfig.ClientId));
            }

            if (string.IsNullOrEmpty(OAuthAuthenticationConfig.ClientSecret))
            {
                throw new ArgumentNullException(nameof(OAuthAuthenticationConfig.ClientSecret));
            }

            //todo get the auth handler
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", OAuthAuthenticationConfig.ClientId);
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", OAuthAuthenticationConfig.ClientSecret);

            var authProviderResponse = await client.PostAsync(OAuthAuthenticationConfig.Uri, new StringContent("", Encoding.UTF32, "application/json-patch+json"));

            if (authProviderResponse.StatusCode != HttpStatusCode.Created || authProviderResponse.Content == null)
            {
                throw new Exception("didn't get a token from the provider");
            }

            var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
            var stsResult = JsonConvert.DeserializeObject<OAuthAuthenticationToken>(responseContent);

            client.DefaultRequestHeaders.Clear();
            client.SetBearerToken(stsResult.AccessToken);

            OAuthAuthenticationToken = stsResult;
        }
    }
}
