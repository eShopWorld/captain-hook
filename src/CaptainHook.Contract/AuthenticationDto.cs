using System.Runtime.Serialization;

namespace CaptainHook.Contract
{
    [KnownType(typeof(OidcAuthenticationDto))]
    [KnownType(typeof(BasicAuthenticationDto))]
    public abstract class AuthenticationDto
    {
        public string Type { get; set; }
    }
}
