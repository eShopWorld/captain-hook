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
    public class MmAuthenticationHandler : AuthenticationHandler
    {
        public MmAuthenticationHandler(
            AuthenticationConfig config,
            IBigBrother bigBrother)
            : base(config, bigBrother)
        { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            if (string.IsNullOrEmpty(Config.ClientId))
            {
                throw new ArgumentNullException(nameof(Config.ClientId));
            }

            if (string.IsNullOrEmpty(Config.ClientId))
            {
                throw new ArgumentNullException(nameof(Config.ClientSecret));
            }

            if (string.IsNullOrEmpty(Config.ClientId))
            {
                throw new ArgumentNullException(nameof(Config.Uri), "Uri is not valid for token service request");
            }

            //todo get the auth handler
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", Config.ClientId);
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", Config.ClientSecret);

            var authProviderResponse = await client.PostAsync(Config.Uri, new StringContent("", Encoding.UTF32, "application/json-patch+json"));

            if (authProviderResponse.StatusCode == HttpStatusCode.Created && authProviderResponse.Content != null)
            {
                var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
                var stsResult = JsonConvert.DeserializeObject<AuthToken>(responseContent);

                client.DefaultRequestHeaders.Clear();
                client.SetBearerToken(stsResult.AccessToken);
                return;
            }
            throw new Exception("didn't get a token from the provider");
        }
    }
}
