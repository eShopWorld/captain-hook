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
        public EndpointSubdocument[] Endpoints { get; set; }

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string SelectionRule { get; set; }

        /// <summary>
        /// Defines Uri transformation definition
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public UriTransformSubdocument UriTransform { get; set; }
    }
}
