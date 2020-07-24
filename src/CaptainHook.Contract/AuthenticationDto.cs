using System.Collections.Generic;

namespace CaptainHook.Contract
{
    public class AuthenticationDto
    {
        public string Type { get; set; }
        public string ClientId { get; set; }
        public string Uri { get; set; }
        public ClientSecretDto ClientSecret { get; set; }
        public List<string> Scopes { get; set; }
    }
}