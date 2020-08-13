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
            var toDelete = selector == null 
                ? _endpoints.SingleOrDefault(x => x.Selector == null) 
                : _endpoints.SingleOrDefault(x => x.Selector != null && x.Selector.Equals(selector, StringComparison.InvariantCultureIgnoreCase));

            return toDelete != null && _endpoints.Remove(toDelete);
        }
    }
}
