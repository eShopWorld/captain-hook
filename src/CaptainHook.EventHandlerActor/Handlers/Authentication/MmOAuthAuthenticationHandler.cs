using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Common;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public class MmOAuthAuthenticationHandler : OAuthAuthenticationHandler
    {
        public MmOAuthAuthenticationHandler(
            AuthenticationConfig authenticationConfig,
            IBigBrother bigBrother)
            : base(authenticationConfig, bigBrother)
        { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            if (string.IsNullOrEmpty(AuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(AuthenticationConfig.ClientId));
            }

            if (string.IsNullOrEmpty(AuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(AuthenticationConfig.ClientSecret));
            }

            if (string.IsNullOrEmpty(AuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(AuthenticationConfig.Uri), "Uri is not valid for token service request");
            }

            //todo get the auth handler
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", AuthenticationConfig.ClientId);
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", AuthenticationConfig.ClientSecret);

            var authProviderResponse = await client.PostAsync(AuthenticationConfig.Uri, new StringContent("", Encoding.UTF32, "application/json-patch+json"));

            if (authProviderResponse.StatusCode == HttpStatusCode.Created && authProviderResponse.Content != null)
            {
                var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
                var stsResult = JsonConvert.DeserializeObject<OAuthAuthenticationToken>(responseContent);

                client.DefaultRequestHeaders.Clear();
                client.SetBearerToken(stsResult.AccessToken);
                return;
            }
            throw new Exception("didn't get a token from the provider");
        }
    }
}
