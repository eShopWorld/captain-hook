using System.Collections.Generic;

namespace CaptainHook.Contract
{
    public class AuthenticationDto
    {
        public string Type { get; set; }

        public string Ref { get; set; }

        public List<string> Scopes { get; set; }
    }
}