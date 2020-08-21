using System.Collections.Generic;

namespace CaptainHook.Contract
{
    public class OidcAuthenticationDto : AuthenticationDto
    {
        public string ClientId { get; set; }
        public string Uri { get; set; }
        public string ClientSecretKeyName { get; set; }
        public List<string> Scopes { get; set; }
    }
}