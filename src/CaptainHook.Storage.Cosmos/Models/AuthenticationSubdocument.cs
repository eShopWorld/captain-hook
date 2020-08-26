using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Authentication model in cosmos db
    /// </summary>
    internal abstract class AuthenticationSubdocument
    {
        /// <summary>
        /// Authentication type
        /// </summary>
        [JsonProperty("type")]
        public abstract string AuthenticationType { get; }
    }
}
