using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class WebhookRequestRuleBuilder
    {
        private ParserLocation source;
        private ParserLocation destination;
        private List<WebhookConfigRoute> routes;

        public WebhookRequestRuleBuilder WithSource(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            this.source = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        public WebhookRequestRuleBuilder WithDestination(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            this.destination = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        public WebhookRequestRuleBuilder AddRoute(string selector, string uri)
        {
            if (this.routes == null)
            {
                this.routes = new List<WebhookConfigRoute>();
            }

            var route = new WebhookConfigRoute
            {
                Selector = selector,
                Uri = uri,
                AuthenticationConfig = new BasicAuthenticationConfig(),
                HttpVerb = "POST"
            };
            this.routes.Add(route);

            return this;
        }

        public WebhookRequestRule Create()
        {
            var rule = new WebhookRequestRule
            {
                Source = this.source,
                Destination = this.destination,
                Routes = this.routes
            };

            return rule;
        }
    }
}