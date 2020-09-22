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
        public string Id { get; set; }

        /// <summary>
        /// Type of the document.
        /// </summary>
        [JsonProperty("type")]
        public string DocumentType { get; set; } = Type;

        /// <summary>
        /// Partition key
        /// </summary>
        public string Pk => GetPartitionKey(EventName);

        /// <summary>
        /// Subscriber name
        /// </summary>
        public string SubscriberName { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        public string EventName { get; set; }

        /// <summary>
        /// Webhooks
        /// </summary>
        public WebhookSubdocument Webhooks { get; set; }

        /// <summary>
        /// Callbacks
        /// </summary>
        public WebhookSubdocument Callbacks { get; set; }

        /// <summary>
        /// DlqHooks
        /// </summary>
        public WebhookSubdocument DlqHooks { get; set; }

        /// <summary>
        /// ETag of the document from Cosmos Db
        /// </summary>
        [JsonProperty("_etag")]
        public string Etag { get; set; }
    }
}
