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

            var selectorSource = routeAndReplaceRule.Source.Replace[SelectorKeyName];
            var selector = string.Empty;

            if (routeAndReplaceRule.Source.Location == Location.Body)
            {
                selector = ModelParser.ParsePayloadPropertyAsString(selectorSource, payload);
            }

            var uri = GetUri(routeAndReplaceRule, selector);
            var replacementDictionary = BuildReplacementDictionary(routeAndReplaceRule.Source.Replace, payload);

            return new BuildUriContext(uri, message => PublishUnroutableEvent(config, message, selector))
                .ApplyReplace(replacementDictionary)
                .CheckIfRoutableAndReturn();
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
                        yield return new KeyValuePair<string, string>(key, valueFromPayload);
                    }
                }
            }

            return new Dictionary<string, string>(RetrieveReplacements());
        }

        private string GetUri(WebhookRequestRule rule, string selector)
        {
            var route = rule.Routes.FirstOrDefault(r => r.Selector.Equals(selector, StringComparison.OrdinalIgnoreCase));
            if (route == null)
            {
                var defaultRoute =
                    rule.Routes.First(r => r.Selector.Equals(DefaultFallbackSelectorKey, StringComparison.OrdinalIgnoreCase));
                return defaultRoute.Uri;
            }

            return route.Uri;
        }
    }
}