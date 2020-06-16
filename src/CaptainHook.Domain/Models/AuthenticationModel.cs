using System.Collections;

namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Authentication model
    /// </summary>
    public class AuthenticationModel
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The secret storage
        /// </summary>
        public SecretStoreModel SecretStore { get; }

        /// <summary>
        /// Authentication URI
        /// </summary>
        public string Uri { get; }

        /// <summary>
        /// Authentication type
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Authentication scopes
        /// </summary>
        public string[] Scopes { get; }

        public AuthenticationModel(string clientId, SecretStoreModel secretStore, string uri, string type, string[] scopes)
        {
            ClientId = clientId;
            SecretStore = secretStore;
            Uri = uri;
            Type = type;
            Scopes = scopes;
        }
    }
}
