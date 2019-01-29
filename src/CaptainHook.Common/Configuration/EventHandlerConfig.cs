using System;
using System.Collections.Generic;
using CaptainHook.Common.Authentication;

namespace CaptainHook.Common.Configuration
{
    /// <summary>
    /// Webhook config contains details for the webhook, eg uri and auth details
    /// </summary>
    public class WebhookConfig
    {
        public WebhookConfig()
        {
            AuthenticationConfig = new AuthenticationConfig();
        }

        public AuthenticationConfig AuthenticationConfig { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        //todo implement this on the calls to the webhook to select http verb
        public string Verb { get; set; }

        /// <summary>
        /// //todo remove this in v1
        /// </summary>
        [Obsolete]
        public string ModelToParse { get; set; }
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

    /// <summary>
    /// Action for the event parser to be preformed on the webhook request or on the callback request
    /// </summary>
    public enum ActionPreformedOn
    {
        Webhook = 1,
        Callback = 2,
        Message = 3
    }

    public class EventParser
    {
        /// <summary>
        ///  Whether to preform the action on the webhook or the callback
        /// </summary>
        public ActionPreformedOn ActionPreformedOn { get; set; }

        /// <summary>
        /// ie from payload, header, etc etc
        /// </summary>
        public ParserLocation Source { get; set; }

        /// <summary>
        /// ie uri, body, header
        /// </summary>
        public ParserLocation Destination { get; set; }

        /// <summary>
        /// Name for reference
        /// </summary>
        public string Name { get; set; }
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
