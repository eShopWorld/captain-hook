namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Subscriber model
    /// </summary>
    public class SubscriberModel
    {
        /// <summary>
        /// Subscriber name
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parent event for this subscriber
        /// </summary>
        public EventModel ParentEvent { get; private set; }

        /// <summary>
        /// Collection of webhook enpoints
        /// </summary>
        public WebhooksModel Webhooks { get; }

        /// <summary>
        /// Collection of callback enpoints
        /// </summary>
        public WebhooksModel Callbacks { get; }

        /// <summary>
        /// Collection of DLQ callback endpoints
        /// </summary>
        public WebhooksModel Dlqs { get; }

        public SubscriberModel(string name) : this(name, null, null, null, null) { }
        public SubscriberModel(string name, string webHookSelector, string callbackSelector, string dlqSelector) : this(name, webHookSelector, callbackSelector, dlqSelector, null) { }
        public SubscriberModel(string name, string webHookSelector, string callbackSelector, string dlqSelector, EventModel parentEvent)
        {
            Name = name;
            Webhooks = new WebhooksModel(webHookSelector);
            Callbacks = new WebhooksModel(callbackSelector);
            Dlqs = new WebhooksModel(dlqSelector);

            SetParentEvent(parentEvent);
        }

        public void SetParentEvent(EventModel parentEvent)
        {
            ParentEvent = parentEvent;
        }

        /// <summary>
        /// Adds an enpoint to the list of webhook endpoints
        /// </summary>
        /// <param name="endpointModel"></param>
        public void AddWebhookEndpoint(EndpointModel endpointModel)
        {
            endpointModel.SetParentSubscriber(this);
            Webhooks.AddEndpoint(endpointModel);
        }

        /// <summary>
        /// Adds an enpoint to the list of webhook endpoints
        /// </summary>
        /// <param name="endpointModel"></param>
        public void AddCallbackEndpoint(EndpointModel endpointModel)
        {
            endpointModel.SetParentSubscriber(this);
            Callbacks.AddEndpoint(endpointModel);
        }

        /// <summary>
        /// Adds an enpoint to the list of webhook endpoints
        /// </summary>
        /// <param name="endpointModel"></param>
        public void AddDlqEndpoint(EndpointModel endpointModel)
        {
            endpointModel.SetParentSubscriber(this);
            Dlqs.AddEndpoint(endpointModel);
        }
    }
}
