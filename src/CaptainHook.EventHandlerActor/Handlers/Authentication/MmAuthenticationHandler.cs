using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Common;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public class MmAuthenticationHandler : AuthenticationHandler
    {
        public MmAuthenticationHandler(
            AuthenticationConfig config)
            : base(config)
        { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
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
