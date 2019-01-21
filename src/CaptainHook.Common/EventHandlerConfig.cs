using System.Collections.Generic;

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

        //todo implement this on the calls to the webhook to select http verb
        public string Verb { get; set; }
    }

    /// <summary>
    /// Event handler config contains both details for the webhook call as well as any domain events and callback
    /// </summary>
    public class EventHandlerConfig
    {
        public WebhookConfig WebHookConfig { get; set; }

        public WebhookConfig CallbackConfig { get; set; }

        public List<EventParser> EventParsers { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool CallBackEnabled => CallbackConfig != null;
    }

    public class EventParser
    {
        /// <summary>
        /// name of the domain event
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// DomainEventPath within the payload to query to get data for delivery
        /// </summary>
        public string ModelQueryPath { get; set; }

        /// <summary>
        /// ie from payload, header, etc etc
        /// </summary>
        public ParserLocation Source { get; set; }

        /// <summary>
        /// ie uri, body, header
        /// </summary>
        public ParserLocation Destination { get; set; }
    }

    public class ParserLocation
    {
        public string Name { get; set; }

        public QueryLocation QueryLocation { get; set; }
    }
    
    public enum QueryLocation
    {
        Uri = 1,
        Body = 2,
        Header = 3,
    }
}
