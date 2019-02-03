using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using IdentityModel.Client;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    /// <summary>
    /// OAuth2 authentication handler.
    /// Gets a token from the supplied STS details included the supplied scopes.
    /// Requests token once
    /// </summary>
    public class OAuthTokenHandler : AuthenticationHandler, IAcquireTokenHandler
    {
        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        protected OAuthAuthenticationToken OAuthAuthenticationToken = new OAuthAuthenticationToken();
        protected readonly OAuthAuthenticationConfig OAuthAuthenticationConfig;

        public OAuthTokenHandler(AuthenticationConfig authenticationConfig)
        {
            var oAuthAuthenticationToken = authenticationConfig as OAuthAuthenticationConfig;
            OAuthAuthenticationConfig = oAuthAuthenticationToken ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(OAuthAuthenticationConfig)}", nameof(authenticationConfig));
        }

        /// <summary>
        /// Gets a token from the STS based on the supplied credentials and scopes using the client grant OAuth 2 Flow
        /// This method also does token renewal based on requesting a token if the token is set to expire in the next ten seconds.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual async Task GetToken(HttpClient client)
        {
            //get initial access token and refresh token
            if (OAuthAuthenticationToken.AccessToken == null)
            {
                var response = await GetTokenResponse(client);

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            }
            else
            {
                await RefreshToken(client);
            }

            client.SetBearerToken(OAuthAuthenticationToken.AccessToken);
        }

        /// <summary>
        /// Makes the call to get the token
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<TokenResponse> GetTokenResponse(HttpMessageInvoker client)
        {
            var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = OAuthAuthenticationConfig.Uri,
                ClientId = OAuthAuthenticationConfig.ClientId,
                ClientSecret = OAuthAuthenticationConfig.ClientSecret,
                GrantType = OAuthAuthenticationConfig.GrantType,
                Scope = string.Join(" ", OAuthAuthenticationConfig.Scopes)
            });
            return response;
        }

        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        protected void UpdateToken(TokenResponse response)
        {
            OAuthAuthenticationToken.AccessToken = response.AccessToken;
            OAuthAuthenticationToken.RefreshToken = response.RefreshToken;
            OAuthAuthenticationToken.ExpiresIn = response.ExpiresIn;
        }

        /// <summary>
        /// Gets a new token from the STS
        /// OAuth refresh flow is not supported in the STS
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual async Task RefreshToken(HttpClient client)
        {
            if (OAuthAuthenticationToken.ExpireTime.Subtract(TimeSpan.FromSeconds(OAuthAuthenticationConfig.RefreshBeforeInSeconds)) <= DateTime.UtcNow)
            {
                var response = await GetTokenResponse(client);

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            }
        }
    }
}
