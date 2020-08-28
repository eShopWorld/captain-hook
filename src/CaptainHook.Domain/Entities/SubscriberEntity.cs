using System;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
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
        /// Gets the version of the entity.
        /// </summary>
        public string Etag { get; }

        /// <summary>
        /// Parent event for this subscriber
        /// </summary>
        public EventEntity ParentEvent { get; private set; }

        /// <summary>
        /// Collection of webhook enpoints
        /// </summary>
        public WebhooksEntity Webhooks { get; private set; }

        public SubscriberEntity(string name, EventEntity parentEvent = null, string etag = null)
        {
            Name = name;
            Etag = etag;
            SetParentEvent(parentEvent);
        }

        public void SetParentEvent(EventEntity parentEvent)
        {
            ParentEvent = parentEvent;
        }

        /// <summary>
        /// Adds an endpoint to the list of webhook endpoints if it is not on the list already.
        /// Removes the existing endpoint and adds the new one to the list if the item is already present.
        /// </summary>
        /// <remarks>The identification is made on selector using case-insensitive comparison.</remarks>
        public OperationResult<SubscriberEntity> SetWebhookEndpoint(EndpointEntity entity)
            => OperationOnWebhooks(webhooks => webhooks.SetEndpoint(entity.SetParentSubscriber(this)));

        /// <summary>
        /// Removes the existing endpoint from the list if the item is present.
        /// </summary>
        /// <remarks>The identification is made on selector using case-insensitive comparison.</remarks>
        public OperationResult<SubscriberEntity> RemoveWebhookEndpoint(EndpointEntity entity)
            => OperationOnWebhooks(webhooks => webhooks.RemoveEndpoint(entity));

        private OperationResult<SubscriberEntity> OperationOnWebhooks(Func<WebhooksEntity, OperationResult<WebhooksEntity>> funcToRun)
        {
            if (Webhooks == null)
            {
                Webhooks = new WebhooksEntity();
            }

            var webhooksResult = funcToRun(Webhooks);

            if (webhooksResult.IsError)
            {
                return webhooksResult.Error switch
                {
                    EndpointNotFoundInSubscriberError notFound => new EndpointNotFoundInSubscriberError(notFound.Selector, this),
                    CannotRemoveLastEndpointFromSubscriberError _ => new CannotRemoveLastEndpointFromSubscriberError(this),
                    _ => webhooksResult.Error
                };
            }

            return this;
        }

        public SubscriberEntity AddWebhooks(WebhooksEntity webhooks)
        {
            foreach (var webhooksEndpoint in webhooks.Endpoints)
            {
                webhooksEndpoint.SetParentSubscriber(this);
            }

            Webhooks = new WebhooksEntity(webhooks.SelectionRule, webhooks.Endpoints, webhooks.UriTransform);

            return this;
        }
    }
}
