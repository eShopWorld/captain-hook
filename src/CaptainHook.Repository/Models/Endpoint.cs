namespace CaptainHook.Repository.Models
{
    public class Endpoint: Entity
    {
        public string Uri { get; set; }
        public string Selector { get; set; }
        public string Authentication { get; set; }
        public string HttpVerb { get; set; }
        public string WebhookSelector { get; set; }
        public string SubscriberName { get; set; }
        public WebhookType WebhookType { get; set; }
        public string EventType { get; set; }
        public string EventName { get; set; }
    }
}
