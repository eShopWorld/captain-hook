using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Entities.Comparers;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Webhooks model
    /// </summary>
    public class WebhooksEntity
    {
        private static readonly ValidationErrorBuilder ValidationErrorBuilder = new ValidationErrorBuilder();

        private static readonly SelectorEndpointEntityEqualityComparer SelectorEndpointEntityEqualityComparer = new SelectorEndpointEntityEqualityComparer();

        private static readonly IValidator<WebhooksEntity> WebhooksValidator = new WebhooksEntityValidator();

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string SelectionRule { get; private set; }

        /// <summary>
        /// URI transformations
        /// </summary>
        public UriTransformEntity UriTransform { get; private set; }

        /// <summary>
        /// Payload Transformation
        /// </summary>
        public string PayloadTransform { get; private set; }

        /// <summary>
        /// Type of entity
        /// </summary>
        public WebhooksEntityType Type { get; }

        /// <summary>
        /// Tells whether there's any endpoint
        /// </summary>
        public bool IsEmpty => Endpoints.Count == 0;

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public IList<EndpointEntity> Endpoints { get; private set; } = new List<EndpointEntity>();

        public WebhooksEntity(WebhooksEntityType type)
        {
            Type = type;
        }

        public WebhooksEntity(WebhooksEntityType type, string selectionRule, IEnumerable<EndpointEntity> endpoints) : this(type, selectionRule, endpoints, null) { }

        public WebhooksEntity(WebhooksEntityType type, string selectionRule, IEnumerable<EndpointEntity> endpoints, UriTransformEntity uriTransform,
            string payloadTransform = null) : this(type)
        {
            SelectionRule = selectionRule;
            Endpoints = endpoints?.ToList();
            UriTransform = uriTransform;
            PayloadTransform = (type == WebhooksEntityType.Callbacks) ? null : (payloadTransform ?? "$");
        }

        public WebhooksEntity SetSelectionRule(string selectionRule)
        {
            SelectionRule = selectionRule;
            return this;
        }

        public WebhooksEntity SetUriTransform(UriTransformEntity uriTransform)
        {
            UriTransform = uriTransform;
            return this;
        }

        public OperationResult<WebhooksEntity> SetEndpoint(EndpointEntity endpoint)
        {
            var extendedEndpointsCollection = Endpoints
                .Except(new[] { endpoint }, SelectorEndpointEntityEqualityComparer)
                .Concat(new[] { endpoint });

            var endpointsEntityForValidation = new WebhooksEntity(Type, SelectionRule, extendedEndpointsCollection, UriTransform, PayloadTransform);

            var validationResult = WebhooksValidator.Validate(endpointsEntityForValidation);

            if (!validationResult.IsValid)
            {
                return ValidationErrorBuilder.Build(validationResult);
            }

            var toDelete = FindMatchingEndpoint(endpoint);
            if (toDelete != null)
            {
                Endpoints.Remove(toDelete);
            }

            Endpoints.Add(endpoint);
            return this;
        }

        public void SetPayloadTransform(string payloadTransform)
        {
            if (Type != WebhooksEntityType.Callbacks)
            {
                PayloadTransform = payloadTransform;
            }
        }

        public OperationResult<WebhooksEntity> SetHooks(WebhooksEntity webhooks, SubscriberEntity subscriberEntity = null)
        {
            SetSelectionRule(webhooks.SelectionRule);
            SetUriTransform(webhooks.UriTransform);
            SetPayloadTransform(webhooks.PayloadTransform);
            Endpoints.Clear();
            foreach (var endpoint in webhooks.Endpoints)
            {
                endpoint.SetParentSubscriber(subscriberEntity);
                var result = SetEndpoint(endpoint);
                if (result.IsError)
                {
                    return result.Error;
                }
            }
            return this;
        }

        public OperationResult<WebhooksEntity> RemoveEndpoint(EndpointEntity endpoint)
        {
            if (Type == WebhooksEntityType.Webhooks && Endpoints.Count == 1)
            {
                return new CannotRemoveLastEndpointFromSubscriberError();
            }

            var toDelete = FindMatchingEndpoint(endpoint);
            if (toDelete == null)
            {
                return new EndpointNotFoundInSubscriberError(endpoint.Selector);
            }

            Endpoints.Remove(toDelete);

            return this;
        }

        private EndpointEntity FindMatchingEndpoint(EndpointEntity endpoint)
            => Endpoints.SingleOrDefault(x => SelectorEndpointEntityEqualityComparer.Equals(x, endpoint));
    }
}
