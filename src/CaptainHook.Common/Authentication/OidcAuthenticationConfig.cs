namespace CaptainHook.Common.Authentication
{
    /// <summary>
    /// OAuth2 Authentication Config
    /// </summary>
    public class OidcAuthenticationConfig : AuthenticationConfig
    {
        public OidcAuthenticationConfig()
        {
            Type = AuthenticationType.OIDC;
        }

        public string Uri { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets it from keyvault
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string[] Scopes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string GrantType { get; } = "client_credentials";

        /// <summary>
        /// Refresh interval before the token expires
        /// </summary>
        public int RefreshBeforeInSeconds { get; set; } = 10;
    }
}
