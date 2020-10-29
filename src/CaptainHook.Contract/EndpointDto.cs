using System;
using Newtonsoft.Json;

namespace CaptainHook.Contract
{
    /// <summary>
    /// Defines a webhook endpoint
    /// </summary>
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class EndpointDto
    {
        private static readonly AuthenticationDto NoAuthentication = new NoAuthenticationDto();
       
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
        public AuthenticationDto Authentication { get; set; } = NoAuthentication;

        /// <summary>
        /// Selector
        /// </summary>
        public string Selector { get; set; }

        /// <summary>
        /// Http call timeout. When not provided the default is 1m 20s.
        /// </summary>
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Http call retries and wait time between those. When not provided the default of 2 additional retries is used, the first one of 20s and the second 30s.
        /// </summary>
        public TimeSpan[] RetrySleepDurations { get; set; }
    }
}
