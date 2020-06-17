using Newtonsoft.Json;
using System;

namespace CaptainHook.Repository.Models
{
    /// <summary>
    /// Endpoint model in cosmos db
    /// </summary>
    public class Endpoint
    {
        public const string Type = "Endpoint";
        public static string GetPartitionKey(string eventName, string subscriberName) => $"{Type}_{eventName}_{subscriberName}";
        public static string GetDocumentId() => Guid.NewGuid().ToString();

        /// <summary>
        /// Endpoint id
        /// </summary>
        [JsonProperty("id")]
        public string Id => GetDocumentId();

        /// <summary>
        /// Partition key
        /// </summary>
        [JsonProperty("pk")]
        public string Pk => GetPartitionKey(EventName, SubscriberName);

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
        public Authentication Authentication { get; set; }

        /// <summary>
        /// Endpoint HTTP verb
        /// </summary>
        public string HttpVerb { get; set; }

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string WebhookSelector { get; set; }

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
