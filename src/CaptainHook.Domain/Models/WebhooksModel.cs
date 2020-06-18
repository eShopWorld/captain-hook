using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Domain.Models
{
    /// <summary>
    /// Webhooks model
    /// </summary>
    public class WebhooksModel
    {
        private readonly List<EndpointModel> _endpoints = new List<EndpointModel>();

        /// <summary>
        /// Webhook selector
        /// </summary>
        public string SelectionRule { get; }

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public IEnumerable<EndpointModel> Endpoints => _endpoints;

        public WebhooksModel(string selectionRule): this(selectionRule, null) { }

        public WebhooksModel(string selectionRule, IEnumerable<EndpointModel> endpoints)
        {
            SelectionRule = selectionRule;
            _endpoints = endpoints?.ToList() ?? new List<EndpointModel>();
        }

        public void AddEndpoint(EndpointModel endpointModel)
        {
            _endpoints.Add(endpointModel);
        }
    }
}
