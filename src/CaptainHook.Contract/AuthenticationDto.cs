using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace CaptainHook.Contract
{
    [KnownType(typeof(OidcAuthenticationDto))]
    [KnownType(typeof(BasicAuthenticationDto))]
    [JsonConverter(typeof(AuthenticationDtoJsonConverter))]
    public abstract class AuthenticationDto
    {
        [JsonProperty("type")]
        public string AuthenticationType { get; set; }
    }
}
