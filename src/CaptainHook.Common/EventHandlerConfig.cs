namespace CaptainHook.Common
{
    /// <summary>
    /// Webhook config contains details for the webhook, eg uri and auth details
    /// </summary>
    public class WebhookConfig
    {
        public bool RequiresAuth { get; set; } = true;

        public AuthConfig AuthConfig { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        //todo implement this on the calls to the webhook
        public string HttpVerb { get; set; }
    }

    /// <summary>
    /// Event handler config contains both details for the webhook call as well as any domain events and callback
    /// </summary>
    public class EventHandlerConfig
    {
        public WebhookConfig WebHookConfig { get; set; }

        public WebhookConfig CallbackConfig { get; set; }

        public EventConfig EventConfig { get; set; }

        public string Name { get; set; }

        public bool CallBackEnabled => CallbackConfig != null;
    }

    public class EventConfig
    {
        /// <summary>
        /// name of the domain event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DomainEventPath within the payload to query to get data for delivery
        /// </summary>
        public string ModelQueryPath { get; set; }
    }
}
