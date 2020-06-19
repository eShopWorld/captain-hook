using System.Collections.Generic;

namespace CaptainHook.Api.Dtos
{
    public class WebhooksDto
    {
        public string SelectionRule { get; set; }

        public List<EndpointDto> Endpoints { get; set; }
    }
}
