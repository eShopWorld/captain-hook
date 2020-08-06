using System.Collections.Generic;

namespace CaptainHook.Contract
{
    /// <summary>
    /// Defines a webhook endpoint
    /// </summary>
    public class EndpointDto
    {
        /// <summary>
        /// Webhook URI
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// HTTP verb for delivery
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// Authentication type and details
        /// </summary>
        public AuthenticationDto Authentication { get; set; }

        /// <summary>
        /// URI transformation
        /// </summary>
        public UriTransformDto UriTransform { get; set; }

        /// <summary>
        /// Selector
        /// </summary>
        public string Selector { get; set; }
    }
}
