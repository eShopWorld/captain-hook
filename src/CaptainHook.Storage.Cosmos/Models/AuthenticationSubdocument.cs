using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Authentication model in cosmos db
    /// </summary>
    [KnownType(typeof(OidcAuthenticationSubdocument))]
    [KnownType(typeof(BasicAuthenticationSubdocument))]
    internal abstract class AuthenticationSubdocument
    {
        /// <summary>
        /// Authentication type
        /// </summary>
        [JsonProperty("type")]
        public abstract string AuthenticationType { get; }
    }
}
