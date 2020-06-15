using System.Collections.Generic;

namespace CaptainHook.Domain.Models
{
    public class Webhooks
    {
        private readonly List<Endpoint> _endpoints = new List<Endpoint>();

        public Subscriber Subscriber { get; set; }

        public string Selector { get; set; }

        public IList<Endpoint> Endpoints => _endpoints;

        public void AddEndpoint(Endpoint endpoint)
        {
            _endpoints.Add(endpoint);
        }
    }
}
