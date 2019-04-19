using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Telemetry;
using Eshopworld.Core;
using IdentityModel.Client;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// OAuth2 authentication handler.
    /// Gets a token from the supplied STS details included the supplied scopes.
    /// Requests token once
    /// </summary>
    public class OidcAuthenticationHandler : AuthenticationHandler, IAcquireTokenHandler
    {
        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        protected OidcAuthenticationToken OidcAuthenticationToken = new OidcAuthenticationToken();
        protected readonly OidcAuthenticationConfig OidcAuthenticationConfig;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        protected readonly IBigBrother BigBrother;

        public OidcAuthenticationHandler(AuthenticationConfig authenticationConfig, IBigBrother bigBrother)
        {
            var oAuthAuthenticationToken = authenticationConfig as OidcAuthenticationConfig;
            OidcAuthenticationConfig = oAuthAuthenticationToken ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(OidcAuthenticationConfig)}", nameof(authenticationConfig));
            BigBrother = bigBrother;
        }

        /// <inheritdoc />
        public virtual async Task GetTokenAsync(HttpClient client, CancellationToken cancellationToken)
        {
            //get initial access token and refresh token
            if (OidcAuthenticationToken.AccessToken == null)
            {
                await EnterSemaphore(cancellationToken, async () =>
                {
                    var response = await GetTokenResponseAsync(client, cancellationToken);
                    ReportTokenUpdateFailure(response);
                    UpdateToken(response);
                });
            }
            else if (CheckExpired())
            {
                await EnterSemaphore(cancellationToken, async () =>
                {
                    if (CheckExpired())
                    {
                        var response = await GetTokenResponseAsync(client, cancellationToken);
                        ReportTokenUpdateFailure(response);
                        UpdateToken(response);
                    }
                });
            }

            client.SetBearerToken(OidcAuthenticationToken.AccessToken);
        }

        /// <summary>
        /// Makes the call to get the token
        /// </summary>
        /// <param name="client"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<TokenResponse> GetTokenResponseAsync(HttpMessageInvoker client, CancellationToken token)
        {
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = OidcAuthenticationConfig.Uri,
                ClientId = OidcAuthenticationConfig.ClientId,
                ClientSecret = OidcAuthenticationConfig.ClientSecret,
                GrantType = OidcAuthenticationConfig.GrantType,
                Scope = string.Join(" ", OidcAuthenticationConfig.Scopes)
            }, token);

            BigBrother.Publish(new ClientTokenRequest
            {
                ClientId = OidcAuthenticationConfig.ClientId,
                Authority = OidcAuthenticationConfig.Uri
            });

            return response;
        }

        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        protected void UpdateToken(TokenResponse response)
        {
            OidcAuthenticationToken.AccessToken = response.AccessToken;
            OidcAuthenticationToken.RefreshToken = response.RefreshToken;
            OidcAuthenticationToken.ExpiresIn = response.ExpiresIn;
            OidcAuthenticationToken.TimeOfRefresh = ServerDateTime.UtcNow;
        }

        /// <summary>
        /// Quackulation to determine when to renew the token
        /// </summary>
        /// <returns></returns>
        private bool CheckExpired()
        {
            return ServerDateTime.UtcNow.Subtract(OidcAuthenticationToken.TimeOfRefresh).TotalSeconds >= OidcAuthenticationToken.ExpiresIn - OidcAuthenticationConfig.RefreshBeforeInSeconds;
        }

        private async Task EnterSemaphore(CancellationToken cancellationToken, Func<Task> action)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken);

                await action();
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
