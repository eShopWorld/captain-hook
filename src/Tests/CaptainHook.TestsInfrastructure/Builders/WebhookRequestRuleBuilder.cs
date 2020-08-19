using System;
using System.Collections.Generic;
using CaptainHook.Common.Configuration;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class WebhookRequestRuleBuilder
    {
        private SourceParserLocation _source;
        private ParserLocation _destination;
        private List<WebhookConfigRoute> _routes;

        public WebhookRequestRuleBuilder WithSource(Action<SourceParserLocationBuilder> sourceBuilder)
        {
            var builder = new SourceParserLocationBuilder();
            sourceBuilder(builder);
            _source = builder.Create();
            return this;
        }

        public WebhookRequestRuleBuilder WithDestination(
            string path = null,
            DataType type = DataType.Property,
            Location location = Location.Body,
            RuleAction ruleAction = RuleAction.Add)
        {
            _destination = new ParserLocation
            {
                Path = path,
                Location = location,
                Type = type,
                RuleAction = ruleAction
            };

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