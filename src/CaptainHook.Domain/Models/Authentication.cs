using System.Collections;

namespace CaptainHook.Domain.Models
{
    public class Authentication
    {
        public string ClientId { get; set; }
        public string Secret { get; set; }
        public string Uri { get; set; }
        public string[] Scopes { get; set; }
    }
}
