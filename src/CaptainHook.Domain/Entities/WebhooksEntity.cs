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

        private readonly IValidator<WebhooksEntity> WebhooksValidator;

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string SelectionRule { get; private set; }

        /// <summary>
        /// URI transformations
        /// </summary>
        public UriTransformEntity UriTransform { get; private set; }

        /// <summary>
        /// Type of entity
        /// </summary>
        public WebhooksEntityType Type { get; }

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public IList<EndpointEntity> Endpoints { get; private set; } = new List<EndpointEntity>();

        public WebhooksEntity(WebhooksEntityType type)
        {
            Type = type;
            WebhooksValidator = new WebhooksEntityValidator(type);
        }

        public WebhooksEntity(WebhooksEntityType type, string selectionRule, IEnumerable<EndpointEntity> endpoints) : this(type, selectionRule, endpoints, null) { }

        public WebhooksEntity(WebhooksEntityType type, string selectionRule, IEnumerable<EndpointEntity> endpoints, UriTransformEntity uriTransform) : this(type)
        {
            SelectionRule = selectionRule;
            Endpoints = endpoints?.ToList();
            UriTransform = uriTransform;
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
                .Except(new [] { endpoint }, SelectorEndpointEntityEqualityComparer)
                .Concat(new[] { endpoint });
            var endpointsEntityForValidation = new WebhooksEntity(Type, SelectionRule, extendedEndpointsCollection, UriTransform);

            var validationResult = WebhooksValidator.Validate(endpointsEntityForValidation);

            if (! validationResult.IsValid)
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
