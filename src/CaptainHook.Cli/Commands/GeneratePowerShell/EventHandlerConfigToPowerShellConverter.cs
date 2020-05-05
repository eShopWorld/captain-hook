using System.Collections.Generic;
using CaptainHook.Cli.Commands.GeneratePowerShell.Internal;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly PsCommandList commands = new PsCommandList();

        public IEnumerable<string> Convert(IEnumerable<EventHandlerConfig> events)
        {
            int eventId = 1;
            foreach (var eventConfig in events)
            {
                commands.Add($"event--{eventId}--type", eventConfig.Type);
                commands.Add($"event--{eventId}--name", eventConfig.Name);

                var webhookPrefix = $"event--{eventId}--webhookconfig";
                AddWebhookDetails(eventConfig.WebhookConfig, webhookPrefix);
                AddWebHookRules(eventConfig.WebhookConfig?.WebhookRequestRules, webhookPrefix);

                var callbackPrefix = $"event--{eventId}--callbackconfig";
                AddCallbackDetails(eventConfig.CallbackConfig, callbackPrefix);
                AddAuthenticationConfigLines(eventConfig.CallbackConfig?.AuthenticationConfig, callbackPrefix);
                AddWebHookRules(eventConfig.CallbackConfig?.WebhookRequestRules, callbackPrefix);

                AddSubscribers(eventConfig.Subscribers, $"event--{eventId}--subscribers");

                eventId++;
            }

            return commands.ToCommandLines();
        }

        private void AddWebhookDetails(WebhookConfig webhookConfig, string webhookPrefix)
        {
            commands.Add($"{webhookPrefix}--name", webhookConfig?.Name);
            commands.Add($"{webhookPrefix}--uri", webhookConfig?.Uri);
            AddAuthenticationConfigLines(webhookConfig?.AuthenticationConfig, webhookPrefix);
            commands.Add($"{webhookPrefix}--httpverb", webhookConfig?.HttpVerb);
        }

        private void AddCallbackDetails(WebhookConfig callbackConfig, string calbackPrefix)
        {
            commands.Add($"{calbackPrefix}--name", callbackConfig?.Name);
        }

        private void AddSubscribers(List<SubscriberConfiguration> subscribers, string subscriberPrefix)
        {
            int subscriberId = 1;
            foreach (var subscriber in subscribers)
            {
                AddSubscriberDetails(subscriber, $"{subscriberPrefix}--{subscriberId}");
                subscriberId++;
            }
        }

        private void AddSubscriberDetails(SubscriberConfiguration subscriber, string subscriberPrefix)
        {
            commands.Add($"{subscriberPrefix}--type", subscriber.EventType);
            commands.Add($"{subscriberPrefix}--name", subscriber.Name);
            commands.Add($"{subscriberPrefix}--subscribername", subscriber.SubscriberName);
            commands.Add($"{subscriberPrefix}--SourceSubscriptionName", subscriber.SourceSubscriptionName);
            commands.Add($"{subscriberPrefix}--dlqmode", subscriber.DLQMode);

            AddWebHookRules(subscriber.WebhookRequestRules, subscriberPrefix);
        }

        private void AddWebHookRules(List<WebhookRequestRule> rules, string prefix)
        {
            if (rules == null)
                return;

            int ruleId = 1;
            foreach (var rule in rules)
            {
                string rulePrefix = $"{prefix}--webhookrequestrules--{ruleId}";

                commands.Add($"{rulePrefix}--Source--path", rule.Source.Path);
                commands.Add($"{rulePrefix}--Source--type", rule.Source.Type);
                commands.Add($"{rulePrefix}--destination--type", rule.Destination.Type);
                commands.Add($"{rulePrefix}--destination--path", rule.Destination.Path);
                commands.Add($"{rulePrefix}--destination--ruleaction", rule.Destination.RuleAction.ToString().ToLower());
                commands.Add($"{rulePrefix}--destination--location", rule.Destination.Location);

                AddRoutes(rule.Routes, rulePrefix);

                ruleId++;
            }
        }

        private void AddRoutes(List<WebhookConfigRoute> routes, string rulePrefix)
        {
            if (routes == null)
                return;

            int routeId = 1;
            foreach (var route in routes)
            {
                string routePrefix = $"{rulePrefix}--routes--{routeId}";

                commands.Add($"{routePrefix}--uri", route.Uri);
                commands.Add($"{routePrefix}--selector", route.Selector);
                commands.Add($"{routePrefix}--httpverb", route.HttpVerb);

                AddAuthenticationConfigLines(route.AuthenticationConfig, routePrefix);

                routeId++;
            }
        }

        private void AddAuthenticationConfigLines(AuthenticationConfig authenticationConfig, string prefix)
        {
            if (authenticationConfig is OidcAuthenticationConfig oidcAuthConfig)
            {
                commands.Add($"{prefix}--authenticationconfig--type", (int)oidcAuthConfig.Type, true);
                commands.Add($"{prefix}--authenticationconfig--uri", oidcAuthConfig.Uri);
                commands.Add($"{prefix}--authenticationconfig--clientid", oidcAuthConfig.ClientId);
                commands.Add($"{prefix}--authenticationconfig--clientsecret", oidcAuthConfig.ClientSecret);
                commands.Add($"{prefix}--authenticationconfig--scopes", oidcAuthConfig.Scopes);
            }
        }
    }
}