namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common;
    using IdentityModel.Client;

    public class AuthenticationHandler : IAuthHandler
    {
        protected readonly AuthenticationConfig Config;

        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        private readonly AuthToken _token = new AuthToken();

        public AuthenticationHandler(AuthenticationConfig config)
        {
            Config = config;
        }

        /// <summary>
        /// This may vary a lot depending on the implementation of each customers auth system
        /// Ideally they implement OIDC/oAuth2 and with credentials we get an access token and refresh token.
        /// Access token may expire after one time use or after a period of time. Refresh is used to get a new access token.
        /// Or they may not give a refresh token at all...annoying. Override, implement and inject as needed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task GetToken(HttpClient client)
        {
            //get initial access token and refresh token
            if (_token.AccessToken == null)
            {
                var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest 
                {
                    Address = Config.Uri,
                    ClientId = Config.ClientId,
                    ClientSecret = Config.ClientSecret,
                    GrantType = Config.GrantType,
                    Scope = Config.Scopes
                });

                if (response.IsError)
                {
                    //todo do something about failed login to get an access/refresh token
                }

                UpdateToken(response);
            }

            //get a new access token from the refresh token
            if (_token.ExpiresTime <= DateTime.UtcNow)
            {
                var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = Config.Uri,
                    RefreshToken = _token.RefreshToken
                });

                if (response.IsError)
                {
                    //todo do something about failed login to get an access/refresh token
                }

                UpdateToken(response);
            }

            client.SetBearerToken(_token.AccessToken);
        }

        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        private void UpdateToken(TokenResponse response)
        {
            _token.AccessToken = response.AccessToken;
            _token.RefreshToken = response.RefreshToken;
            _token.ExpiresIn = response.ExpiresIn;
        }
    }
}
