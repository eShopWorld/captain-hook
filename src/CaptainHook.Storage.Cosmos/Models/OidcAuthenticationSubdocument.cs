using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// OIDC authentication model in cosmos db
    /// </summary>
    internal class OidcAuthenticationSubdocument : AuthenticationSubdocument
    {
        public const string Type = "OIDC";

        /// <summary>
        /// Authentication type
        /// </summary>
        [JsonProperty("type")]
        public override string AuthenticationType => Type;

        /// <summary>
        /// Client id for authentication
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Name of the secret holding the actual secret in the referenced keyvault
        /// </summary>
        public string SecretName { get; set; }

        /// <summary>
        /// URI for authentication
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// List of scopes for this authentication client
        /// </summary>
        public string[] Scopes { get; set; }
    }
}
