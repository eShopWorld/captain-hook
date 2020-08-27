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
        private static readonly IValidator<WebhooksEntity> WebhooksValidator = new WebhooksEntityValidator();

        private static readonly ValidationErrorBuilder ValidationErrorBuilder = new ValidationErrorBuilder();

        private static readonly SelectorEndpointEntityEqualityComparer SelectorEndpointEntityEqualityComparer = new SelectorEndpointEntityEqualityComparer();

        private readonly IList<EndpointEntity> _endpoints;

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string SelectionRule { get; }

        /// <summary>
        /// URI transformations
        /// </summary>
        public UriTransformEntity UriTransform { get; }

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public IEnumerable<EndpointEntity> Endpoints => _endpoints;

        public WebhooksEntity()
            : this(null, null, null)
        {
        }

        public WebhooksEntity(string selectionRule) : this(selectionRule, null, null) { }

        public WebhooksEntity(string selectionRule, IEnumerable<EndpointEntity> endpoints) : this(selectionRule, endpoints, null) { }

        public WebhooksEntity(string selectionRule, IEnumerable<EndpointEntity> endpoints, UriTransformEntity uriTransform)
        {
            SelectionRule = selectionRule;
            UriTransform = uriTransform;
            _endpoints = endpoints?.ToList() ?? new List<EndpointEntity>();
        }

        public OperationResult<WebhooksEntity> SetEndpoint(EndpointEntity endpoint)
        {
            var extendedEndpointsCollection = _endpoints
                .Except(new [] { endpoint }, SelectorEndpointEntityEqualityComparer)
                .Concat(new[] { endpoint });
            var endpointsEntityForValidation = new WebhooksEntity(SelectionRule, extendedEndpointsCollection, UriTransform);

            var validationResult = WebhooksValidator.Validate(endpointsEntityForValidation);

            if (! validationResult.IsValid)
            {
                return ValidationErrorBuilder.Build(validationResult);
            }

            var toDelete = FindMatchingEndpoint(endpoint);
            if (toDelete != null)
            {
                _endpoints.Remove(toDelete);
            }

            _endpoints.Add(endpoint);
            return this;
        }

        public OperationResult<WebhooksEntity> RemoveEndpoint(EndpointEntity endpoint)
        {
            if (_endpoints.Count == 1)
            {
                return new CannotRemoveLastEndpointFromSubscriberError();
            }

            var toDelete = FindMatchingEndpoint(endpoint);
            if (toDelete == null)
            {
                return new EndpointNotFoundInSubscriberError(endpoint.Selector);
            }

            _endpoints.Remove(toDelete);

            return this;
        }

        private EndpointEntity FindMatchingEndpoint(EndpointEntity endpoint)
            => _endpoints.SingleOrDefault(x => SelectorEndpointEntityEqualityComparer.Equals(x, endpoint));
    }
}
