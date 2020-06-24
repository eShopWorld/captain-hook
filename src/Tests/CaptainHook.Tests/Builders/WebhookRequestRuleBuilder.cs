using System;
using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Tests.Builders
{
    internal class WebhookRequestRuleBuilder
    {
        private ParserLocation _source;
        private ParserLocation _destination;
        private List<WebhookConfigRoute> _routes;

        public WebhookRequestRuleBuilder WithSource(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            _source = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        public WebhookRequestRuleBuilder WithDestination(string path = null, DataType type = DataType.Property, Location location = Location.Body)
        {
            _destination = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
            };

            return this;
        }

        // TODO: remove or use version of this method which accepts WebhookConfigRouteBuilder
        public WebhookRequestRuleBuilder AddRoute(string selector, string uri)
        {
            if (_routes == null)
            {
                _routes = new List<WebhookConfigRoute>();
            }

            var route = new WebhookConfigRoute
            {
                Selector = selector,
                Uri = uri,
                AuthenticationConfig = new BasicAuthenticationConfig(),
                HttpVerb = "POST"
            };
            _routes.Add(route);

            return this;
        }

        public WebhookRequestRuleBuilder AddRoute(Action<WebhookConfigRouteBuilder> routeBuilder)
        {
            if (_routes == null)
            {
                _routes = new List<WebhookConfigRoute>();
            }

            var builder = new WebhookConfigRouteBuilder();
            routeBuilder(builder);
            _routes.Add(builder.Create());
            return this;
        }

        public WebhookRequestRule Create()
        {
            var rule = new WebhookRequestRule
            {
                Source = _source,
                Destination = _destination,
                Routes = _routes
            };

            return rule;
        }
    }
}