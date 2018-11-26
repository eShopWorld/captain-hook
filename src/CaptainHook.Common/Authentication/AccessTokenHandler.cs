namespace CaptainHook.Common.Authentication
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using IdentityModel.Client;

    public class AccessTokenHandler : IAccessTokenHandler
    {
        private readonly HttpClient _client;
        private readonly AuthConfig _config;

        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        private readonly AuthToken _token = new AuthToken();

        public AccessTokenHandler(HttpClient client, AuthConfig config)
        {
            _client = client;
            _config = config;
        }

        /// <summary>
        /// This may vary a lot depending on the implementation of each customers auth system
        /// Ideally they implement OIDC/oAuth2 and with credentials we get an access token and refresh token.
        /// Access token may expire after one time use or after a period of time. Refresh is used to get a new access token.
        /// Or they may not give a refresh token at all...annoying. Override, implement and inject as needed.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<string> GetToken()
        {
            //get initial access token and refresh token
            if (_token.AccessToken == null)
            {
                var response = await _client.RequestTokenAsync(new TokenRequest
                {
                    Address = _config.Uri,
                    ClientId = _config.ClientId,
                    ClientSecret = _config.ClientSecret,
                });

                if (response.IsError)
                {
                    //todo do something about failed login to get an access/refresh token
                }

                UpdateToken(response);

            }

            //get a new access token from the refresh token
            if (_token.ExpiresTime >= DateTime.UtcNow)
            {
                var response = await _client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = _config.Uri,
                    RefreshToken = _token.RefreshToken
                });

                if (response.IsError)
                {
                    //todo do something about failed login to get an access/refresh token
                }

                UpdateToken(response);
            }

            return _token.AccessToken;
        }

        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        private void UpdateToken(TokenResponse response)
        {
            _token.AccessToken = response.AccessToken;
            _token.RefreshToken = response.RefreshToken;
            _token.Update(response.ExpiresIn);
        }
    }
}
