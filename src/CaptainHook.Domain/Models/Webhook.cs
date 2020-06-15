using System.Collections.Generic;

namespace CaptainHook.Domain.Models
{
    public class Webhook
    {
        public string Selector { get; set; }
        public IList<Endpoint> Endpoints { get; set; } = new List<Endpoint>();
    }
}
