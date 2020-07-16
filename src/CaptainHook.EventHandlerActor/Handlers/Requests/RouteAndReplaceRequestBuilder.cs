using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class RouteAndReplaceRequestBuilder: DefaultRequestBuilder
    {
        private const string SelectorKeyName = "selector";

        private const string DefaultFallbackSelectorKey = "*";

        public RouteAndReplaceRequestBuilder(IBigBrother bigBrother): base(bigBrother)
        {
        }

        protected override Uri BuildUriFromExistingConfig(WebhookConfig config, string payload)
        {
            var routeAndReplaceRule = config.WebhookRequestRules.First(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            var selector = GetRouteSelector(payload, routeAndReplaceRule);
            var webhookConfig = GetWebhookConfig(routeAndReplaceRule, selector);

            var replacementDictionary = BuildReplacementDictionary(routeAndReplaceRule.Source.Replace, payload);

            return new BuildUriContext(
                    webhookConfig.Uri,
                    message => PublishUnroutableEvent(config, message, selector))
                .ApplyReplace(replacementDictionary)
                .CheckIfRoutableAndReturn();
        }

        protected override WebhookConfig SelectWebhookConfigCore(WebhookConfig webhookConfig, string payload)
        {
            var routeAndReplaceRule = webhookConfig.WebhookRequestRules.First(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            var selector = GetRouteSelector(payload, routeAndReplaceRule);
            var routeConfig = GetWebhookConfig(routeAndReplaceRule, selector);

            return routeConfig;
        }

        private IDictionary<string, string> BuildReplacementDictionary(IDictionary<string, string> sourceReplace, string payload)
        {
            string RetrieveValueFromPayload(string propertyPath)
            {
                try
                {
                    return ModelParser.ParsePayloadPropertyAsString(propertyPath, payload);
                }
                catch (Exception ex)
                {
                    // send unroutable event instead
                    BigBrother.Publish(ex.ToExceptionEvent());
                }

                return null;
            }

            IEnumerable<KeyValuePair<string, string>> RetrieveReplacements()
            {
                foreach (var (key, value) in sourceReplace)
                {
                    var valueFromPayload = RetrieveValueFromPayload(value);
                    if (!string.IsNullOrEmpty(valueFromPayload))
                    {
                        var escapedValue = Uri.EscapeDataString(valueFromPayload);
                        yield return new KeyValuePair<string, string>(key, escapedValue);
                    }
                }
            }

            return new Dictionary<string, string>(RetrieveReplacements());
        }

        private WebhookConfigRoute GetWebhookConfig(WebhookRequestRule rule, string selector)
        {
            var route = rule.Routes.FirstOrDefault(r => r.Selector.Equals(selector, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                var defaultRoute =
                    rule.Routes.First(r => r.Selector.Equals(DefaultFallbackSelectorKey, StringComparison.OrdinalIgnoreCase));
                return defaultRoute;
            }

            return route;
        }

        private static string GetRouteSelector(string payload, WebhookRequestRule routeAndReplaceRule)
        {
            var selectorSource = routeAndReplaceRule.Source.Replace[SelectorKeyName];
            var selector = string.Empty;

            if (routeAndReplaceRule.Source.Location == Location.Body)
            {
                selector = ModelParser.ParsePayloadPropertyAsString(selectorSource, payload);
            }

            return selector;
        }
    }
}