using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        [JsonProperty("uri")]
        public string Uri { get; set; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        [JsonProperty("selector", NullValueHandling = NullValueHandling.Ignore)]
        public string Selector { get; set; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        [JsonProperty("authentication")]
        public AuthenticationData Authentication { get; set; }

        /// <summary>
        /// Endpoint HTTP verb
        /// </summary>
        [JsonProperty("httpVerb")]
        public string HttpVerb { get; set; }

        /// <summary>
        /// Endpoint type (Webhook, Subscriber, DLQ)
        /// </summary>
        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointType Type { get; set; }

        /// <summary>
        /// Defines Uri transformation definition
        /// </summary>
        [JsonProperty("uriTransform", NullValueHandling = NullValueHandling.Ignore)]
        public UriTransformDocument UriTransform { get; set; }
    }
}
