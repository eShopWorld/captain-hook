using System;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using IdentityModel.Client;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public class BasicAuthenticationToken
    {
        public string Username { get; set; }

        public string Password { get; set; }

        private string _encodedToken;

        public bool IsEmpty => Username == null || Password == null;

        /// <summary>
        /// Gets the encoded token
        /// </summary>
        public string EncodedToken
        {
            get
            {
                if (_encodedToken == null)
                {
                    if (string.IsNullOrWhiteSpace(Username))
                    {
                        throw new ArgumentException("value needs to be populated correctly", nameof(Username));
                    }

                    if (string.IsNullOrWhiteSpace(Username))
                    {
                        throw new ArgumentException("value needs to be populated correctly", nameof(Password));
                    }

                    _encodedToken = Convert.ToBase64String(System.Text.Encoding.GetEncoding("UTF-8").GetBytes(Username + ":" + Password));
                }

                return _encodedToken;
            }
        }
    }

    public class BasicAuthenticationHandler : IAuthenticationHandler
    {
        private AuthenticationConfig authenticationConfig;

        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        private readonly BasicAuthenticationToken _authenticationToken = new BasicAuthenticationToken();
        
        public BasicAuthenticationHandler(AuthenticationConfig authenticationConfig)
        {
            authenticationConfig = authenticationConfig;
        }

        public virtual Task GetToken(HttpClient client)
        {
            if (_authenticationToken.IsEmpty)
            {
                _authenticationToken.Username = 

                var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest
                {
                    Address = AuthenticationConfig.Uri,
                    ClientId = AuthenticationConfig.ClientId,
                    ClientSecret = AuthenticationConfig.ClientSecret,
                    GrantType = AuthenticationConfig.GrantType,
                    Scope = AuthenticationConfig.Scopes
                });
            }
        }
        
        protected void ReportTokenUpdateFailure(TokenResponse response)
        {
            if (!response.IsError)
            {
                return;
            }
            throw new Exception($"Unable to get access token from STS. Error = {response.ErrorDescription}");
        }
    }

    /// <summary>
    /// OAuth2 authentication handler.
    /// Gets a token from the supplied STS details included the supplied scopes.
    /// Requests token once
    /// </summary>
    public class OAuthAuthenticationHandler : BasicAuthenticationHandler
    {
        //todo cache and make it thread safe, ideally should have one per each auth domain and have the expiry set correctly
        private readonly OAuthAuthenticationToken _authenticationToken = new OAuthAuthenticationToken();

        public OAuthAuthenticationHandler(AuthenticationConfig authenticationConfig) : base(authenticationConfig)
        {
            AuthenticationConfig = authenticationConfig;
        }

        /// <summary>
        /// This may vary a lot depending on the implementation of each customers auth system
        /// Ideally they implement OIDC/oAuth2 and with credentials we get an access token and refresh token.
        /// Access token may expire after one time use or after a period of time. Refresh is used to get a new access token.
        /// Or they may not give a refresh token at all...annoying. Override, implement and inject as needed.
        /// </summary>
        /// <returns></returns>
        public override async Task GetToken(HttpClient client)
        {
            //get initial access token and refresh token
            if (_authenticationToken.AccessToken == null)
            {
                var response = await client.RequestTokenAsync(new ClientCredentialsTokenRequest 
                {
                    Address = AuthenticationConfig.Uri,
                    ClientId = AuthenticationConfig.ClientId,
                    ClientSecret = AuthenticationConfig.ClientSecret,
                    GrantType = AuthenticationConfig.GrantType,
                    Scope = AuthenticationConfig.Scopes
                });

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            }
            if (_authenticationToken.ExpireTime.Add(-TimeSpan.FromSeconds(10d)) >= DateTime.UtcNow)
            {
                var response = await client.RequestRefreshTokenAsync(new RefreshTokenRequest
                {
                    Address = AuthenticationConfig.Uri,
                    RefreshToken = _authenticationToken.RefreshToken
                });

                ReportTokenUpdateFailure(response);
                UpdateToken(response);
            }

            client.SetBearerToken(_authenticationToken.AccessToken);
        }
        
        /// <summary>
        /// Updates the local cached token
        /// </summary>
        /// <param name="response"></param>
        private void UpdateToken(TokenResponse response)
        {
            _authenticationToken.AccessToken = response.AccessToken;
            _authenticationToken.RefreshToken = response.RefreshToken;
            _authenticationToken.ExpiresIn = response.ExpiresIn;
        }
    }
}
