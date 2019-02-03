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
    public class MmAuthenticationHandler : OidcAuthenticationHandler
    {
        public MmAuthenticationHandler(AuthenticationConfig authenticationConfig) 
            : base(authenticationConfig)
        {

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            if (string.IsNullOrEmpty(OidcAuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(OidcAuthenticationConfig.ClientId));
            }

            if (string.IsNullOrEmpty(OidcAuthenticationConfig.ClientSecret))
            {
                throw new ArgumentNullException(nameof(OidcAuthenticationConfig.ClientSecret));
            }

            //todo get the auth handler
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", OidcAuthenticationConfig.ClientId);
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", OidcAuthenticationConfig.ClientSecret);

            var authProviderResponse = await client.PostAsync(OidcAuthenticationConfig.Uri, new StringContent("", Encoding.UTF32, "application/json-patch+json"));

            if (authProviderResponse.StatusCode != HttpStatusCode.Created || authProviderResponse.Content == null)
            {
                throw new Exception("didn't get a token from the provider");
            }

            var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
            var stsResult = JsonConvert.DeserializeObject<OidcAuthenticationToken>(responseContent);

            client.DefaultRequestHeaders.Clear();
            client.SetBearerToken(stsResult.AccessToken);

            OidcAuthenticationToken = stsResult;
        }
    }
}
