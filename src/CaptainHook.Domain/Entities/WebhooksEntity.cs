using System;
using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Domain.Entities
{
    /// <summary>
    /// Webhooks model
    /// </summary>
    public class WebhooksEntity
    {
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

        public void AddEndpoint(EndpointEntity endpointModel)
        {
            _endpoints.Add(endpointModel);
        }

        public bool RemoveEndpoint(string selector)
        {
            var toDelete = _endpoints.SingleOrDefault(x => string.Equals(x.Selector, selector, StringComparison.InvariantCultureIgnoreCase));
            return toDelete != null && _endpoints.Remove(toDelete);
        }
    }
}
