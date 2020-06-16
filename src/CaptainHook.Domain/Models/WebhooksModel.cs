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
        public string Selector { get; }

        /// <summary>
        /// Webhook endpoints
        /// </summary>
        public IEnumerable<EndpointModel> Endpoints => _endpoints;

        public WebhooksModel(string selector): this(selector, null) { }

        public WebhooksModel(string selector, IEnumerable<EndpointModel> endpoints)
        {
            Selector = selector;
            _endpoints = endpoints?.ToList() ?? new List<EndpointModel>();
        }

        public void AddEndpoint(EndpointModel endpointModel)
        {
            _endpoints.Add(endpointModel);
        }
    }
}
