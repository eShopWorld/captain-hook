namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Newtonsoft.Json;

    public class MmAuthHandler : AuthHandler
    {
        public MmAuthHandler(
            AuthConfig config)
            : base(config)
        { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            //todo get the auth handler
            client.DefaultRequestHeaders.Add("Content-Type", "application/json-patch+json; charset=utf-8");
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_id", Config.ClientId);
            client.DefaultRequestHeaders.TryAddWithoutValidation("client_secret", Config.ClientSecret);

            var authProviderResponse = await client.PostAsync(Config.Uri, null);

            if (authProviderResponse.StatusCode == HttpStatusCode.Created && authProviderResponse.Content != null)
            {
                var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
                var stsResult = JsonConvert.DeserializeObject<AuthToken>(responseContent);

                client.SetBearerToken(stsResult.AccessToken);
            }
            //todo handle token failure
            throw new Exception("didn't get a token from the provider");
        }
    }
}