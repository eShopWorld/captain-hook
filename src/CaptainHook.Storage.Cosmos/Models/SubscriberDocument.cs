using CaptainHook.Domain.ValueObjects;
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

        /// <summary>
        /// Identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id;

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
        /// Webhooks
        /// </summary>
        [JsonProperty("eventName")]
        public WebhookSubdocument Webhooks { get; set; }
    }
}
