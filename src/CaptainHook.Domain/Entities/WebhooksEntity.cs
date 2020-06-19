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

        public WebhooksEntity(string selectionRule): this(selectionRule, null) { }

        public WebhooksEntity(string selectionRule, IEnumerable<EndpointEntity> endpoints)
        {
            SelectionRule = selectionRule;
            _endpoints = endpoints?.ToList() ?? (IList<EndpointEntity>)Array.Empty<EndpointEntity>();
        }

        public void AddEndpoint(EndpointEntity endpointModel)
        {
            _endpoints.Add(endpointModel);
        }
    }
}
