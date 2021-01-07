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

        /// <summary>
        /// Flag which indicates that client id/secret are passed in headers and scopes & grant type are ignored
        /// </summary>
        public bool UseHeaders { get; }

        public OidcAuthenticationEntity(string clientId, string clientSecretKeyName, string uri, string[] scopes, bool useHeaders = false)
        {
            ClientId = clientId;
            ClientSecretKeyName = clientSecretKeyName;
            Uri = uri;
            Scopes = scopes;
            UseHeaders = useHeaders;
        }
    }
}
