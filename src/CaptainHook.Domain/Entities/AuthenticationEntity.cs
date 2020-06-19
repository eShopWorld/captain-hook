namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Authentication model
    /// </summary>
    public class AuthenticationEntity
    {
        /// <summary>
        /// Client ID
        /// </summary>
        public string ClientId { get; }

        /// <summary>
        /// The secret storage
        /// </summary>
        public SecretStoreEntity SecretStore { get; }

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

        public AuthenticationEntity(string clientId, SecretStoreEntity secretStore, string uri, string type, string[] scopes)
        {
            ClientId = clientId;
            SecretStore = secretStore;
            Uri = uri;
            Type = type;
            Scopes = scopes;
        }
    }
}
