using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Basic authentication model in cosmos db
    /// </summary>
    internal class BasicAuthenticationSubdocument : AuthenticationSubdocument
    {
        /// <summary>
        /// Username for basic authentication
        /// </summary>
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// Password for basic authentication
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }
    }
}
