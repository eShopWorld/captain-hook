namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Subscriber model
    /// </summary>
    public class SubscriberEntity
    {
        /// <summary>
        /// Unique identifier of the subscriber.
        /// </summary>
        public string SubscriberId { get; }

        /// <summary>
        /// Subscriber name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Parent event for this subscriber
        /// </summary>
        public EventEntity ParentEvent { get; private set; }

        /// <summary>
        /// Collection of webhook enpoints
        /// </summary>
        public WebhooksEntity Webhooks { get; }

        public SubscriberEntity(string subscriberId, string name) : this(subscriberId, name, null, null) { }
        public SubscriberEntity(string subscriberId, string name, string webhookSelectionRule) : this(subscriberId, name, webhookSelectionRule, null) { }
        public SubscriberEntity(string subscriberId, string name, string webhookSelectionRule, EventEntity parentEvent)
        {
            SubscriberId = subscriberId;
            Name = name;
            Webhooks = new WebhooksEntity(webhookSelectionRule);

            SetParentEvent(parentEvent);
        }

        public void SetParentEvent(EventEntity parentEvent)
        {
            ParentEvent = parentEvent;
        }

        /// <summary>
        /// Adds an enpoint to the list of webhook endpoints
        /// </summary>
        /// <param name="endpointModel"></param>
        public void AddWebhookEndpoint(EndpointEntity endpointModel)
        {
            endpointModel.SetParentSubscriber(this);
            Webhooks.AddEndpoint(endpointModel);
        }
    }
}
