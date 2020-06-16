using Newtonsoft.Json;
using System;

namespace CaptainHook.Repository.Models
{
    public class Endpoint
    {
        public const string Type = "Endpoint";
        public static string GetPartitionKey(string eventName, string subscriberName) => $"{eventName}-{subscriberName}";
        public static string GetDocumentId() => Guid.NewGuid().ToString();

        [JsonProperty("id")]
        public string Id => GetDocumentId();

        [JsonProperty("pk")]
        public string Pk => GetPartitionKey(EventName, SubscriberName);

        public string Uri { get; set; }
        public string EndpointSelector { get; set; }
        public Authentication Authentication { get; set; }
        public string HttpVerb { get; set; }
        public string WebhookSelector { get; set; }
        public string SubscriberName { get; set; }
        public WebhookType WebhookType { get; set; }
        public string EventName { get; set; }
    }
}
