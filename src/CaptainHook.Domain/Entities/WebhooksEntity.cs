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
        /// Webhook endpoints
        /// </summary>
        public IEnumerable<EndpointEntity> Endpoints => _endpoints;

        public WebhooksEntity()
            : this(null, null)
        {
        }

        public WebhooksEntity(string selectionRule): this(selectionRule, null) { }

        public WebhooksEntity(string selectionRule, IEnumerable<EndpointEntity> endpoints)
        {
            SelectionRule = selectionRule;
            _endpoints = endpoints?.ToList() ?? new List<EndpointEntity>();
        }

        public void AddEndpoint(EndpointEntity endpointModel)
        {
            _endpoints.Add(endpointModel);
        }

        public bool RemoveEndpoint(string selector)
        {
            var endpoint = _endpoints.SingleOrDefault(x => x.Selector.Equals(selector, StringComparison.InvariantCultureIgnoreCase));
            return endpoint != null && _endpoints.Remove(endpoint);
        }
    }
}
