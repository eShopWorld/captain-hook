namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Basic authentication model in cosmos db
    /// </summary>
    internal class BasicAuthenticationSubdocument : AuthenticationSubdocument
    {
        public const string Type = "Basic";

        /// <summary>
        /// Authentication type
        /// </summary>
        public override string AuthenticationType => Type;

        /// <summary>
        /// Username for basic authentication
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for basic authentication
        /// </summary>
        public string PasswordKeyName { get; set; }
    }
}
