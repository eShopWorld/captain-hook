namespace CaptainHook.Domain.Entities
{
    public class OidcAuthenticationEntity : AuthenticationEntity
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
        /// Authentication scopes
        /// </summary>
        public string[] Scopes { get; }

        public OidcAuthenticationEntity(string clientId, string clientSecretKeyName, string uri, string[] scopes)
        {
            ClientId = clientId;
            ClientSecretKeyName = clientSecretKeyName;
            Uri = uri;
            Scopes = scopes;
        }
    }
}
