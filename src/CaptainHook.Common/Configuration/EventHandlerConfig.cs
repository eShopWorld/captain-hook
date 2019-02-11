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
            WebhookRequestRules = new List<WebhookRequestRule>();
        }

        public AuthenticationConfig AuthenticationConfig { get; set; }

        public string Uri { get; set; }

        public string Name { get; set; }

        //todo implement this on the calls to the webhook to select http verb
        public string HttpVerb { get; set; }

        public List<WebhookRequestRule> WebhookRequestRules { get; set; }
    }

    /// <summary>
    /// Event handler config contains both details for the webhook call as well as any domain events and callback
    /// </summary>
    public class EventHandlerConfig
    {
        public WebhookConfig WebHookConfig { get; set; }

        public WebhookConfig CallbackConfig { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public bool CallBackEnabled => CallbackConfig != null;
    }

    public class WebhookRequestRule
    {
        public WebhookRequestRule()
        {
            Routes = new List<WebhookConfigRoute>();
            Source = new ParserLocation();
            Destination = new ParserLocation();
        }

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

        /// <summary>
        /// Routes used for webhook rule types
        /// </summary>
        public List<WebhookConfigRoute> Routes { get; set; }
    }

    public class WebhookConfigRoute : WebhookConfig
    {
        /// <summary>
        /// A selector that is used in the payload to determine where the request should be routed to in the config
        /// </summary>
        public string Selector { get; set; }
    }

    public class ParserLocation
    {
        [Obsolete]
        public string Name { get; set; }

        /// <summary>
        /// Path for the parameter to query from or to be placed
        /// ie: path in the message both or if it's a value in the http header
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The location of the parsed parameter or the location it should go
        /// </summary>
        public Location Location { get; set; }
    }

    public enum Location
    {
        /// <summary>
        /// Mostly used to add something to the URI of the request
        /// </summary>
        Uri = 1,

        /// <summary>
        /// The request payload body. Can come from or be attached to
        /// </summary>
        PayloadBody = 2,

        /// <summary>
        /// Headers for the requests to add
        /// </summary>
        Header = 3,

        /// <summary>
        /// Query parameters to add, esp for HTTP Gets
        /// </summary>
        QueryParameter = 4,

        /// <summary>
        /// The domain event body, ie where the data can come from
        /// </summary>
        MessageBody = 5,

        /// <summary>
        /// Special case to get the status code of the webhook request and add it to the call back body
        /// </summary>
        StatusCode = 6
    }
}
