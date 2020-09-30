﻿using Newtonsoft.Json;

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
    }
}
