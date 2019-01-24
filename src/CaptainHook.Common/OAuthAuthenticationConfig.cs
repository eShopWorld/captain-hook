namespace CaptainHook.Common
{
    /// <summary>
    /// OAuth2 Authentication Config
    /// </summary>
    public class OAuthAuthenticationConfig : AuthenticationConfig
    {
        /// <summary>
        /// //todo put this in ci authConfig/production authConfig
        /// </summary>
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
    }
}
