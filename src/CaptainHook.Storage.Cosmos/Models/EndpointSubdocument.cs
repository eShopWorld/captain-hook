using System;
using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Endpoint model in Cosmos DB
    /// </summary>
    internal class EndpointSubdocument
    {
        /// <summary>
        /// Endpoint URI
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Selector { get; set; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public AuthenticationSubdocument Authentication { get; set; }

        /// <summary>
        /// Endpoint HTTP verb
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// Http Timeout
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan? Timeout { get; set; }

        /// <summary>
        /// Retry sleep durations
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public TimeSpan[] RetrySleepDurations { get; set; }
    }
}
