using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Telemetry.Web;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// Custom Authentication Handler
    /// </summary>
    public class MmAuthenticationHandler : OidcAuthenticationHandler
    {
        public MmAuthenticationHandler(
            IHttpSender httpSender,
            AuthenticationConfig authenticationConfig,
            IBigBrother bigBrother)
            : base(httpSender, authenticationConfig, bigBrother)
        { }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override async Task<string> GetTokenAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(OidcAuthenticationConfig.ClientId))
            {
                throw new ArgumentNullException(nameof(OidcAuthenticationConfig.ClientId));
            }

            if (string.IsNullOrEmpty(OidcAuthenticationConfig.ClientSecret))
            {
                throw new ArgumentNullException(nameof(OidcAuthenticationConfig.ClientSecret));
            }

            var headers = new WebHookHeaders();
            headers.AddContentHeader(Constants.Headers.ContentType, "application/json-patch+json");
            headers.AddRequestHeader("client_id", OidcAuthenticationConfig.ClientId);
            headers.AddRequestHeader("client_secret", OidcAuthenticationConfig.ClientSecret);

            var defaultRetrySleepDurations = new WebhookConfig().RetrySleepDurations;
            var authProviderResponse = await HttpSender.SendAsync(
                new SendRequest(HttpMethod.Post, new Uri(OidcAuthenticationConfig.Uri), headers, string.Empty, defaultRetrySleepDurations),
                cancellationToken);

            if (authProviderResponse.StatusCode != HttpStatusCode.Created || authProviderResponse.Content == null)
            {
                throw new InvalidOperationException("didn't get a token from the provider");
            }

            var responseContent = await authProviderResponse.Content.ReadAsStringAsync();
            var stsResult = JsonConvert.DeserializeObject<OidcAuthenticationToken>(responseContent);

            OidcAuthenticationToken = stsResult;

            BigBrother.Publish(new ClientTokenRequest
            {
                ClientId = OidcAuthenticationConfig.ClientId,
                Authority = OidcAuthenticationConfig.Uri
            });

            return $"Bearer {stsResult.AccessToken}";
        }
    }
}
