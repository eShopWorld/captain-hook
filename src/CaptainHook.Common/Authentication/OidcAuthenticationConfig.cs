using Newtonsoft.Json;
using System.ComponentModel;

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

        /// <summary>
        /// Gets or sets the URI
        /// </summary>
        [JsonProperty(Order = 3)]
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the client id
        /// </summary>
        [JsonProperty(Order = 2)]
        public string ClientId { get; set; }

        /// <summary>
        /// Gets it from keyvault
        /// </summary>
        [JsonProperty(Order = 4)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the scopes
        /// </summary>
        [JsonProperty(Order = 5)]
        public string[] Scopes { get; set; }

        /// <summary>
        /// Gets or sets the grant type
        /// </summary>
        [DefaultValue("client_credentials")]
        public string GrantType { get; } = "client_credentials";

        /// <summary>
        /// Refresh interval before the token expires
        /// </summary>
        [DefaultValue(10)]
        public int RefreshBeforeInSeconds { get; set; } = 10;
    }
}
