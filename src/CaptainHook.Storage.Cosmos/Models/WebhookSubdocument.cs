using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Webhook model in Cosmos DB
    /// </summary>
    internal class WebhookSubdocument
    {
        /// <summary>
        /// Subscriber endpoints
        /// </summary>
        [JsonProperty(PropertyName = "endpoints")]
        public EndpointSubdocument[] Endpoints { get; set; }

        /// <summary>
        /// Webhook selector
        /// </summary>
        [JsonProperty("selectionRule")]
        public string SelectionRule { get; set; }

        /// <summary>
        /// Defines Uri transformation definition
        /// </summary>
        [JsonProperty("uriTransform", NullValueHandling = NullValueHandling.Ignore)]
        public UriTransformSubdocument UriTransform { get; set; }
    }
}
