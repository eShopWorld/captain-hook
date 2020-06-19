using System.Collections.Generic;

namespace CaptainHook.Api.Dtos
{
    public class AuthenticationDto
    {
        public string Type { get; set; }

        public string Ref { get; set; }

        public List<string> Scopes { get; set; }
    }
}