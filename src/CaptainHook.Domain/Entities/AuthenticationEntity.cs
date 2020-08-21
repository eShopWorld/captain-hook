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
        /// Name of the secret key which holds the actual secret
        /// </summary>
        public string ClientSecretKeyName { get; }

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

        public AuthenticationEntity(string clientId, string clientSecretKeyName, string uri, string type, string[] scopes)
        {
            ClientId = clientId;
            ClientSecretKeyName = clientSecretKeyName;
            Uri = uri;
            Type = type;
            Scopes = scopes;
        }
    }
}
