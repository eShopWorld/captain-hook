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

        public SubscriberModel(string name) : this(name, null, null) { }
        public SubscriberModel(string name, string webhookSelectionRule) : this(name, webhookSelectionRule, null) { }
        public SubscriberModel(string name, string webhookSelectionRule, EventModel parentEvent)
        {
            Name = name;
            Webhooks = new WebhooksModel(webhookSelectionRule);

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
    }
}
