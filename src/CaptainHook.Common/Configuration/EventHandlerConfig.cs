using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.Serialization;
using CaptainHook.Common.Authentication;
using Newtonsoft.Json;

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

        /// <summary>
        /// authentication config for this instance of webhook
        /// </summary>
        [JsonProperty(Order = 3)]
        public AuthenticationConfig AuthenticationConfig { get; set; }

        /// <summary>
        /// target URL
        /// </summary>
        [JsonProperty(Order = 2)]
        public string Uri { get; set; }

        /// <summary>
        /// webhook name
        /// </summary>
        [JsonProperty(Order = 1)]
        public string Name { get; set; }

        /// <summary>
        /// HTTP method to use when executing the call
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public HttpMethod HttpMethod { get; set; } = HttpMethod.Post;

        /// <summary>
        /// due to limitations of binder (simple POCOs), route the config through this property
        /// </summary>
        [JsonProperty(Order = 4)]
        public string HttpVerb
        {
            get => HttpMethod.Method;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                    HttpMethod = new HttpMethod(value);
            }
        }

        /// <summary>
        /// The event type of this event
        /// </summary>
        [JsonIgnore]
        public string EventType { get; set; }

        /// <summary>
        /// The default http content type used for events
        /// </summary>
        [JsonIgnore]
        public string ContentType { get; set; } = Constants.Headers.DefaultContentType;

        /// <summary>
        /// associated rules for the callback
        /// </summary>
        [JsonProperty(Order = 5)]
        public List<WebhookRequestRule> WebhookRequestRules { get; set; }

        /// <summary>
        /// Request duration maximum timeout in seconds
        /// Left at 100 seconds as the default value for the http client timeout
        /// </summary>
        [JsonProperty(Order = 6)]
        public TimeSpan Timeout { get; set; } = new TimeSpan(0, 0, 100);

        /// <summary>
        /// type of transformation to perform onto payload
        /// 
        /// pertains to new callback/DLQ contract designed
        /// </summary>
        [JsonIgnore]
        public PayloadContractTypeEnum PayloadTransformation { get; set; } = PayloadContractTypeEnum.Raw;
    }

    /// <summary>
    /// signifies what kind of contract to use for webhook
    /// </summary>
    public enum PayloadContractTypeEnum
    {
        /// <summary>
        /// use payload directly
        /// </summary>
        Raw = 0,
        /// <summary>
        /// use wrapper contract
        /// </summary>
        WrapperContract = 1
    }

    /// <summary>
    /// Defines the configuration of the subscriber of a topic.
    /// </summary>
    public class SubscriberConfiguration : WebhookConfig
    {
        /// <summary>
        /// Creates a key used to index the subscriber configurations.
        /// </summary>
        /// <param name="typeName">The full name of the type of the message.</param>
        /// <param name="subscriberName">The optional name of the subsciber.</param>
        /// <returns></returns>
        public static string Key(string typeName, string subscriberName)
            => $"{typeName};{subscriberName}".ToLowerInvariant();

        /// <summary>
        /// The configuration of the webhook which should receive the response message from
        /// the main webhook.
        /// </summary>
        [JsonProperty(Order = 8)]
        public WebhookConfig Callback { get; set; }

        /// <summary>
        /// intended name for the subscriber(and therefore subscription)
        /// </summary>
        [JsonProperty(Order = 3)]
        public string SubscriberName { get; set; }

        /// <summary>
        /// signals that subscriber is in DLQ mode and what processing mode is used for incoming message
        /// </summary>
        [JsonProperty(Order = 5)]
        public SubscriberDlqMode? DLQMode { get; set; }

        /// <summary>
        /// source subscription for the this instance of subscription
        /// 
        /// in DLQ mode this is the source subscription to hook DLQ under
        /// </summary>
        [JsonProperty(Order = 4)]
        public string SourceSubscriptionName { get; set; }

        /// <summary>
        /// Specifies a configuration which should be used when the name has not been provided.
        /// </summary>
        /// <remarks>
        /// It's for backward compatiblity only.
        /// </remarks>
        public bool IsMainConfiguration { get; private set; }

        /// <summary>
        /// Provide this value as TimeSpan format to send HeartBeat telemetry event from Event Reader
        /// </summary>
        public string HeartBeatInterval { get; set; }

        /// <summary>
        /// Tranforms the old structure of configuration into the new one.
        /// </summary>
        /// <param name="webhookConfig">The webhook configuration.</param>
        /// <param name="callback">The callback associated with <paramref name="webhookConfig"/>.</param>
        /// <param name="heartBeatInterval">Heart Beat Interval value if applicable.</param>
        /// <returns>The subscriber configuration consisting of the webhook configuration and its callback.</returns>
        public static SubscriberConfiguration FromWebhookConfig(WebhookConfig webhookConfig, WebhookConfig callback, string heartBeatInterval)
        {
            return new SubscriberConfiguration
            {
                AuthenticationConfig = webhookConfig.AuthenticationConfig,
                HttpMethod = webhookConfig.HttpMethod,
                EventType = webhookConfig.EventType,
                ContentType = webhookConfig.ContentType,
                Name = webhookConfig.Name,
                SubscriberName = "captain-hook", //for the legacy config, assume the legacy name as well
                Timeout = webhookConfig.Timeout,
                Uri = webhookConfig.Uri,
                WebhookRequestRules = webhookConfig.WebhookRequestRules,
                Callback = callback,
                IsMainConfiguration = true,
                HeartBeatInterval = heartBeatInterval
            };
        }

        internal int CollectionIndex { get; set; }

        [JsonIgnore]
        public string WebHookConfigPath
        {
            get
            {
                return IsMainConfiguration ? "webhookconfig" : $"subscribers:{CollectionIndex+1}"; //convention is 1-based index
            }
        }

        [JsonIgnore]
        public string CallbackConfigPath
        {
            get
            {
                return IsMainConfiguration ? "callbackconfig" : $"subscribers:{CollectionIndex+1}:callbackconfig"; //convention is 1-based index
            }
        }
    }

    /// <summary>
    /// Event handler config contains both details for the webhook call as well as any domain events and callback
    /// </summary>
    public class EventHandlerConfig
    {
        /// <summary>
        /// The list of all subscibers of the topic handling the event type.
        /// </summary>
        [JsonProperty(Order = 6)]
        public List<SubscriberConfiguration> Subscribers { get; } = new List<SubscriberConfiguration>();

        /// <summary>
        /// Returns all subscribers defined in the old and new configuration schemas.
        /// </summary>
        [JsonIgnore]
        public IEnumerable<SubscriberConfiguration> AllSubscribers
        {
            get
            {
                if (WebhookConfig != null)
                    yield return SubscriberConfiguration.FromWebhookConfig(WebhookConfig, CallbackConfig, HeartBeatInterval);
                foreach (var conf in Subscribers)
                {
                    conf.CollectionIndex = Subscribers.IndexOf(conf);
                    yield return conf;
                }
            }
        }

        /// <summary>
        /// The webhook definition using the old schema of configuration.
        /// </summary>
        [JsonProperty(Order = 3)]
        public WebhookConfig WebhookConfig { get; set; }

        /// <summary>
        /// The callback of associated with <see cref="WebhookConfig"/> using the old schema of configuration.
        /// </summary>
        [JsonProperty(Order = 4)]
        public WebhookConfig CallbackConfig { get; set; }

        [JsonProperty(Order = 2)]
        public string Name { get; set; }

        [JsonProperty(Order = 1)]
        public string Type { get; set; }

        [JsonProperty(Order = 5)]
        public string HeartBeatInterval { get; set; }
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
        /// Routes used for webhook rule types
        /// </summary>
        public List<WebhookConfigRoute> Routes { get; set; }
    }

    public class WebhookConfigRoute : WebhookConfig
    {
        /// <summary>
        /// A selector that is used in the payload to determine where the request should be routed to in the config
        /// </summary>
        [JsonProperty(Order = 3)]
        public string Selector { get; set; }
    }

    public class ParserLocation
    {
        /// <summary>
        /// Path for the parameter to query from or to be placed
        /// ie: path in the message both or if it's a value in the http header
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// The location of the parsed parameter or the location it should go
        /// </summary>
        [DefaultValue(Location.Body)]
        public Location Location { get; set; } = Location.Body;

        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(RuleAction.Add)]
        public RuleAction RuleAction { get; set; } = RuleAction.Add;

        /// <summary>
        /// The data type of the property
        /// </summary>
        [DefaultValue(DataType.Property)]
        public DataType Type { get; set; } = DataType.Property;
    }

    public enum DataType
    {
        Property = 1,
        HttpContent = 2,
        HttpStatusCode = 3,
        Model = 4,
        String = 5
    }

    public enum RuleAction
    {
        Replace = 1,
        Add = 2,
        Route = 3
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
        Body = 2,

        /// <summary>
        /// 
        /// </summary>
        Header = 3,

        /// <summary>
        /// Special case to get the status code of the webhook request and add it to the call back body
        /// </summary>
        HttpStatusCode = 4,

        /// <summary>
        /// Special case to get the status code of the webhook request and add it to the call back body
        /// </summary>
        HttpContent = 5,
    }

    /// <summary>
    /// indicates that subscriber is in DLQ mode and what it is expected to do with incoming message
    /// </summary>
    /// <remarks>
    /// other modes will follow
    /// </remarks>
    public enum SubscriberDlqMode
    {
        /// <summary>
        /// call webhook when DLQ message received
        /// </summary>
        WebHookMode = 1
    }
}
