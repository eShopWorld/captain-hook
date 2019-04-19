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
    public class OidcAuthenticationHandler : AuthenticationHandler, IAcquireTokenHandler
    {
        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        protected OidcAuthenticationToken OidcAuthenticationToken = new OidcAuthenticationToken();
        protected readonly OidcAuthenticationConfig OidcAuthenticationConfig;

        public OidcAuthenticationHandler(AuthenticationConfig authenticationConfig)
        {
            var oAuthAuthenticationToken = authenticationConfig as OidcAuthenticationConfig;
            OidcAuthenticationConfig = oAuthAuthenticationToken ?? throw new ArgumentException($"configuration for basic authentication is not of type {typeof(OidcAuthenticationConfig)}", nameof(authenticationConfig));
        }

        /// <summary>
        /// Gets a token from the STS based on the supplied credentials and scopes using the client grant OIDC 2 Flow
        /// This method also does token renewal based on requesting a token if the token is set to expire in the next ten seconds.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual async Task GetToken(HttpClient client)
        {
            //get initial access token and refresh token
            //if (OidcAuthenticationToken.AccessToken == null)
            //{
                var response = await GetTokenResponse(client);

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            //}
            //else
            //{
            //    await RefreshToken(client);
            //}

            client.SetBearerToken(OidcAuthenticationToken.AccessToken);
        }

        /// <summary>
        /// Makes the call to get the token
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private async Task<TokenResponse> GetTokenResponse(HttpMessageInvoker client)
        {
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = OidcAuthenticationConfig.Uri,
                ClientId = OidcAuthenticationConfig.ClientId,
                ClientSecret = OidcAuthenticationConfig.ClientSecret,
                GrantType = OidcAuthenticationConfig.GrantType,
                Scope = string.Join(" ", OidcAuthenticationConfig.Scopes)
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
        }

        /// <summary>
        /// Gets a new token from the STS
        /// OIDC refresh flow is not supported in the STS
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected virtual async Task RefreshToken(HttpClient client)
        {
            if (OidcAuthenticationToken.ExpireTime.Subtract(TimeSpan.FromSeconds(OidcAuthenticationConfig.RefreshBeforeInSeconds)) <= DateTime.UtcNow)
            {
                var response = await GetTokenResponse(client);

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            }
        }
    }
}
