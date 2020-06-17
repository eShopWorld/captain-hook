using Newtonsoft.Json;

namespace CaptainHook.Repository.Models
{
    /// <summary>
    /// Authentication model in cosmos db
    /// </summary>
    public class Authentication
    {
        /// <summary>
        /// Client id for authentication
        /// </summary>
        [JsonProperty("clientId")]
        public string ClientId { get; set; }

        /// <summary>
        /// Keyvault holding the actual secret
        /// </summary>
        [JsonProperty("keyVaultName")]
        public string KeyVaultName { get; set; }

        /// <summary>
        /// Name of the secret holding the actual secret in the referenced keyvault
        /// </summary>
        [JsonProperty("secretName")]
        public string SecretName { get; set; }

        /// <summary>
        /// Authentication type
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

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
