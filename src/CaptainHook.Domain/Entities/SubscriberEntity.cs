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
        public WebhooksEntity Webhooks { get; private set; }

        public SubscriberEntity(string name) : this(name, null) { }
        public SubscriberEntity(string name, EventEntity parentEvent)
        {
            Name = name;
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
            if (Webhooks == null)
            {
                Webhooks = new WebhooksEntity();
            }

            endpointModel.SetParentSubscriber(this);
            Webhooks.AddEndpoint(endpointModel);

            return this;
        }

        public SubscriberEntity AddWebhooks(WebhooksEntity webhooks)
        {
            foreach (var webhooksEndpoint in webhooks.Endpoints)
            {
                webhooksEndpoint.SetParentSubscriber(this);
            }

            Webhooks = new WebhooksEntity(webhooks.SelectionRule, webhooks.Endpoints);

            return this;
        }
    }
}
