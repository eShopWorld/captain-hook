using Newtonsoft.Json;
using System;

namespace CaptainHook.Repository.Models
{
    /// <summary>
    /// Endpoint model in Cosmos DB
    /// </summary>
    internal class EndpointDocument
    {
        public const string Type = "Endpoint";
        public static string GetPartitionKey(string eventName) => $"{Type}_{eventName}";
        
        //change
        public static string GetDocumentId(string eventName, string subscriberName, string selector) => $"{eventName}_{subscriberName}_{selector}";

        /// <summary>
        /// Identifier
        /// </summary>
        [JsonProperty("id")]
        public string Id => GetDocumentId(EventName, SubscriberName, EndpointSelector);

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
        /// Endpoint URI
        /// </summary>
        [JsonProperty("uri")]
        public string Uri { get; set; }

        /// <summary>
        /// Endpoint selector
        /// </summary>
        [JsonProperty("endpointSelector")]
        public string EndpointSelector { get; set; }

        /// <summary>
        /// Endpoint authentication
        /// </summary>
        [JsonProperty("authentication")]
        public AuthenticationData Authentication { get; set; }

        /// <summary>
        /// Endpoint HTTP verb
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string WebhookSelectionRule { get; set; }

        /// <summary>
        /// Subscriber name
        /// </summary>
        public string SubscriberName { get; set; }

        /// <summary>
        /// Webhook type (Webhook, Subscriber, DLQ)
        /// </summary>
        public WebhookType WebhookType { get; set; }

        /// <summary>
        /// Event name
        /// </summary>
        public string EventName { get; set; }
    }
}
