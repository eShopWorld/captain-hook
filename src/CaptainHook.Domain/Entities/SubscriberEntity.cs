using CaptainHook.Domain.ValueObjects;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Subscriber model
    /// </summary>
    public class SubscriberEntity
    {
        public SubscriberId Id => new SubscriberId(ParentEvent?.Name, Name);

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

        public SubscriberEntity(string name) : this(name, null, null) { }
        public SubscriberEntity(string name, string webhookSelectionRule) : this(name, webhookSelectionRule, null) { }
        public SubscriberEntity(string name, string webhookSelectionRule, EventEntity parentEvent)
        {
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
        public SubscriberEntity AddWebhookEndpoint(EndpointEntity endpointModel)
        {
            endpointModel.SetParentSubscriber(this);
            Webhooks.AddEndpoint(endpointModel);

            return this;
        }
    }
}
