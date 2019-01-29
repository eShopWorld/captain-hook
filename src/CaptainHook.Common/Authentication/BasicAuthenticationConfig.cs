namespace CaptainHook.Common.Authentication
{
    /// <summary>
    /// Basic authentication configuration
    /// </summary>
    public class BasicAuthenticationConfig : AuthenticationConfig
    {
        public BasicAuthenticationConfig()
        {
            this.AuthenticationType = AuthenticationType.Basic;
        }

        /// <summary>
        /// Username for basic auth
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Password for basic auth
        /// </summary>
        public string Password { get; set; }
    }
}
