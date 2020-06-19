namespace CaptainHook.Api.Dtos
{
    public class SubscriberDto
    {
        public string Name { get; set; }

        public string EventName { get; set; }

        public WebhooksDto Webhooks { get; set; }
    }
}
