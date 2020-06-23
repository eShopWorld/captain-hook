using System.Collections.Generic;

namespace CaptainHook.Contract
{
    public class WebhooksDto
    {
        public string SelectionRule { get; set; }

        public List<EndpointDto> Endpoints { get; set; }
    }
}
