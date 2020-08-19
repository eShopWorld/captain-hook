using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Webhook model in Cosmos DB
    /// </summary>
    internal class WebhookDocument
    {
        /// <summary>
        /// Subscriber endpoints
        /// </summary>
        [JsonProperty(PropertyName = "endpoints")]
        public EndpointDocument[] Endpoints { get; set; }

        /// <summary>
        /// Webhook selector
        /// </summary>
        [JsonProperty("selectionRule")]
        public string SelectionRule { get; set; }

        /// <summary>
        /// Defines Uri transformation definition
        /// </summary>
        [JsonProperty("uriTransform", NullValueHandling = NullValueHandling.Ignore)]
        public UriTransformDocument UriTransform { get; set; }
    }
}
