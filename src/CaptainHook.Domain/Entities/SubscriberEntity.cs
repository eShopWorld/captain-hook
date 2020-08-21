using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Subscriber model
    /// </summary>
    public class SubscriberEntity
    {
        private static readonly IValidator<WebhooksEntity> WebhooksValidator = new WebhooksEntityValidator();

        private static readonly ValidationErrorBuilder ValidationErrorBuilder = new ValidationErrorBuilder();

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
        /// Adds an endpoint to the list of webhook endpoints
        /// </summary>
        /// <param name="endpointModel"></param>
        public OperationResult<SubscriberEntity> AddWebhookEndpoint(EndpointEntity endpointModel)
        {
            if (Webhooks == null)
            {
                Webhooks = new WebhooksEntity();
            }

            var extendedEndpointsCollection = Webhooks.Endpoints.Concat(new[] { endpointModel });
            var endpointsEntityForValidation = new WebhooksEntity(Webhooks.SelectionRule, extendedEndpointsCollection, Webhooks.UriTransform);

            var validationResult = WebhooksValidator.Validate(endpointsEntityForValidation);

            if (validationResult.IsValid)
            {
                endpointModel.SetParentSubscriber(this);
                Webhooks.AddEndpoint(endpointModel);

                return this;
            }

            return ValidationErrorBuilder.Build(validationResult);
        }

        public OperationResult<SubscriberEntity> RemoveWebhookEndpoint(string selector)
        {
            if (Webhooks.Endpoints.Count() == 1)
                return new CannotRemoveLastEndpointFromSubscriberError(this);

            if (!Webhooks.RemoveEndpoint(selector))
                return new EndpointNotFoundInSubscriberError(selector, this);

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
