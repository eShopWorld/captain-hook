using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// OIDC authentication model in cosmos db
    /// </summary>
    internal class OidcAuthenticationSubdocument : AuthenticationSubdocument
    {
        /// <summary>
        /// Client id for authentication
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Name of the secret holding the actual secret in the referenced keyvault
        /// </summary>
        [JsonProperty("secretName")]
        public string SecretName { get; set; }

        /// <summary>
        /// URI for authentication
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }

        /// <summary>
        /// List of scopes for this authentication client
        /// </summary>
        [JsonProperty("scopes")]
        public string[] Scopes { get; set; }
    }
}
