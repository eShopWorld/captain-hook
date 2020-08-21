using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace CaptainHook.Contract
{
    [KnownType(typeof(OidcAuthenticationDto))]
    [KnownType(typeof(BasicAuthenticationDto))]
    [JsonConverter(typeof(AuthenticationDtoJsonConverter))]
    public abstract class AuthenticationDto
    {
        public string Type { get; set; }
    }
}
