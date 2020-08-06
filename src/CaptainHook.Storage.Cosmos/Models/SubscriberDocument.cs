using Newtonsoft.Json;

namespace CaptainHook.Storage.Cosmos.Models
{
    /// <summary>
    /// Subscriber model in Cosmos DB
    /// </summary>
    internal class SubscriberDocument
    {
        public const string Type = "Subscriber";
        public static string GetPartitionKey(string eventName) => $"{Type}_{eventName}";

        public static string GetDocumentId(string eventName, string subscriberName) => $"{eventName}-{subscriberName}";

        /// <summary>
        /// Identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id => GetDocumentId(EventName, SubscriberName);

        /// <summary>
        /// Type of the document.
        /// </summary>
        [JsonProperty("type")]
        public string DocumentType { get; set; } = Type;

        /// <summary>
        /// Partition key
        /// </summary>
        [JsonProperty("pk")]
        public string Pk => GetPartitionKey(EventName);

        /// <summary>
        /// Subscriber name
        /// </summary>
        [JsonProperty("subscriberName")]
        public string SubscriberName { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        [JsonProperty("eventName")]
        public string EventName { get; set; }

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
    }
}
