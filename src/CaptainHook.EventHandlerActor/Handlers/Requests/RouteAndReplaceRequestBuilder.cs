using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;
using FluentValidation;

namespace CaptainHook.EventHandlerActor.Handlers.Requests
{
    public class RouteAndReplaceRequestBuilder: DefaultRequestBuilder
    {
        public const string SelectorKeyName = "selector";
        public const string DefaultFallbackSelectorKey = "*";

        private readonly IValidator<WebhookConfig> _validator;

        public RouteAndReplaceRequestBuilder(IBigBrother bigBrother, IValidator<WebhookConfig> validator): base(bigBrother)
        {
            _validator = validator;
        }

        protected override Uri BuildUriFromExistingConfig(WebhookConfig config, string payload)
        {
            void PublishUnroutableEventWithoutSelector(string message) => PublishUnroutableEvent(config, message);

            var result = _validator.Validate(config);
            if (! result.IsValid)
            {
                var message = $"Validation errors for subscriber configuration: {string.Join(", ", result.Errors)}";
                PublishUnroutableEventWithoutSelector(message);
                return null;
            }

            var routeAndReplaceRule = config.WebhookRequestRules.First(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            var selector = GetRouteSelector(payload, routeAndReplaceRule, PublishUnroutableEventWithoutSelector);
            var webhookConfig = GetWebhookConfig(routeAndReplaceRule, selector);

            void PublishUnroutableEventWithMessage(string message) => PublishUnroutableEvent(config, message, selector);

            if (webhookConfig == null)
            {
                PublishUnroutableEventWithMessage($"No route found for selector '{selector}'");
                return null;
            }

            var replacementDictionary = BuildReplacementDictionary(routeAndReplaceRule.Source.Replace, payload, PublishUnroutableEventWithMessage);

            return new BuildUriContext(
                    webhookConfig.Uri,
                    PublishUnroutableEventWithMessage)
                .ApplyReplace(replacementDictionary)
                .CheckIfRoutableAndReturn();
        }

        protected override WebhookConfig SelectWebhookConfigCore(WebhookConfig webhookConfig, string payload)
        {
            var routeAndReplaceRule = webhookConfig.WebhookRequestRules.First(r => r.Destination.RuleAction == RuleAction.RouteAndReplace);
            var selector = GetRouteSelector(payload, routeAndReplaceRule, message => PublishUnroutableEvent(webhookConfig, message));
            var routeConfig = GetWebhookConfig(routeAndReplaceRule, selector);

            return routeConfig;
        }

        public override HttpMethod SelectHttpMethod(WebhookConfig webhookConfig, string payload) =>
            SelectWebhookConfig(webhookConfig, payload).HttpMethod;

        public override WebhookConfig GetAuthenticationConfig(WebhookConfig webhookConfig, string payload) => SelectWebhookConfig(webhookConfig, payload);

        private IDictionary<string, string> BuildReplacementDictionary(
            IDictionary<string, string> sourceReplace,
            string payload,
            Action<string> publishUnroutableEvent)
        {
            IEnumerable<KeyValuePair<string, string>> RetrieveReplacements()
            {
                foreach (var (key, value) in sourceReplace)
                {
                    var (found, valueFromPayload) = RetrieveValueFromPayload(value, payload, publishUnroutableEvent);
                    if (found)
                    {
                        var escapedValue = Uri.EscapeDataString(valueFromPayload ?? string.Empty);
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
                    rule.Routes.FirstOrDefault(r => r.Selector.Equals(DefaultFallbackSelectorKey, StringComparison.OrdinalIgnoreCase));
                return defaultRoute;
            }

            return route;
        }

        private string GetRouteSelector(string payload, WebhookRequestRule routeAndReplaceRule, Action<string> publishUnroutableEvent)
        {
            var selectorSource = routeAndReplaceRule.Source.Replace[SelectorKeyName];
            if (routeAndReplaceRule.Source.Location == Location.Body)
            {
                var (_, value) = RetrieveValueFromPayload(selectorSource, payload, publishUnroutableEvent);
                return value;
            }

            return null;
        }

        private (bool found, string value) RetrieveValueFromPayload(string propertyPath, string payload, Action<string> publishUnroutableEvent)
        {
            try
            {
                var value = ModelParser.ParsePayloadPropertyAsString(propertyPath, payload);
                return (true, value);
            }
            catch (Exception)
            {
                publishUnroutableEvent($"Error looking for {propertyPath} in the message payload");
                return (false, null);
            }
        }
    }
}