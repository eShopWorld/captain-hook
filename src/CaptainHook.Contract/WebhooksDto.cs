using System.Collections.Generic;

namespace CaptainHook.Contract
{
    public class WebhooksDto
    {
        /// <summary>
        /// Webhook selection rule
        /// </summary>
        public string SelectionRule { get; set; }

        /// <summary>
        /// URI transformation
        /// </summary>
        public UriTransformDto UriTransform { get; set; }

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public List<EndpointDto> Endpoints { get; set; }

        /// <summary>
        /// Payload transformation
        /// </summary>
        public string PayloadTransform { get; set; }
    }
}
