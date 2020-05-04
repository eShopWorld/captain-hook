using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly List<string> lines = new List<string>();

        public IEnumerable<string> Convert(IEnumerable<EventHandlerConfig> events)
        {
            int eventId = 1;
            foreach (var eventConfig in events)
            {
                lines.Add($"setConfig 'event--{eventId}--type' '{eventConfig.Type}' $KeyVault");
                lines.Add($"setConfig 'event--{eventId}--name' '{eventConfig.Name}' $KeyVault");

                var webhookPrefix = $"setConfig 'event--{eventId}--webhookconfig";
                AddWebhookDetails(eventConfig.WebhookConfig, webhookPrefix);
                AddWebHookRules(eventConfig.WebhookConfig?.WebhookRequestRules, webhookPrefix);

                var callbackPrefix = $"setConfig 'event--{eventId}--callbackconfig";
                AddCallbackDetails(eventConfig.CallbackConfig, callbackPrefix);
                AddAuthenticationConfigLines(eventConfig.CallbackConfig?.AuthenticationConfig, callbackPrefix);
                AddWebHookRules(eventConfig.CallbackConfig?.WebhookRequestRules, callbackPrefix);

                AddSubscribers(eventConfig.Subscribers, $"setConfig 'event--{eventId}--subscribers");

                eventId++;
            }

            return lines;
        }

        private void AddWebhookDetails(WebhookConfig webhookConfig, string webhookPrefix)
        {
            lines.Add($"{webhookPrefix}--name' '{webhookConfig?.Name}' $KeyVault");
            lines.Add($"{webhookPrefix}--uri' '{webhookConfig?.Uri}' $KeyVault");
            AddAuthenticationConfigLines(webhookConfig?.AuthenticationConfig, webhookPrefix);
            lines.Add($"{webhookPrefix}--httpverb' '{webhookConfig?.HttpVerb}' $KeyVault");
        }

        private void AddCallbackDetails(WebhookConfig callbackConfig, string calbackPrefix)
        {
            lines.Add($"{calbackPrefix}--name' '{callbackConfig?.Name}' $KeyVault");
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
            lines.Add($"{subscriberPrefix}--type' '{subscriber.EventType}' $KeyVault");
            lines.Add($"{subscriberPrefix}--name' '{subscriber.Name}' $KeyVault");
            lines.Add($"{subscriberPrefix}--subscribername' '{subscriber.SubscriberName}' $KeyVault");
            lines.Add($"{subscriberPrefix}--SourceSubscriptionName' '{subscriber.SourceSubscriptionName}' $KeyVault");
            lines.Add($"{subscriberPrefix}--dlqmode' '{ConvertDlqMode(subscriber.DLQMode)}' $KeyVault");

            AddWebHookRules(subscriber.WebhookRequestRules, subscriberPrefix);
        }

        private static string ConvertDlqMode(SubscriberDlqMode? dlqMode)
        {
            if (dlqMode.HasValue)
            {
                var t = (int)dlqMode.Value;
                return t.ToString();
            }

            return string.Empty;
        }

        private void AddWebHookRules(List<WebhookRequestRule> rules, string prefix)
        {
            if (rules == null)
                return;

            int ruleId = 1;
            foreach (var rule in rules)
            {
                string rulePrefix = $"{prefix}--webhookrequestrules--{ruleId}";

                // source--path: just a string
                // source--type: Model, HttpContent, HttpStatusCode, property
                // source--ruleaction: not used
                // source--location: not used
                // destination--path: Content, StatusCode
                // destination--type: Model, String
                // destination--location: Uri
                // destination--ruleaction: route

                lines.Add($"{rulePrefix}--Source--path' '{rule.Source.Path}' $KeyVault");
                lines.Add($"{rulePrefix}--Source--type' '{rule.Source.Type}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--type' '{rule.Destination.Type}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--path' '{rule.Destination.Path}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--ruleaction' '{rule.Destination.RuleAction.ToString().ToLower()}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--location' '{rule.Destination.Location}' $KeyVault");

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

                lines.Add($"{routePrefix}--uri' '{route.Uri}' $KeyVault");
                lines.Add($"{routePrefix}--selector' '{route.Selector}' $KeyVault");
                lines.Add($"{routePrefix}--uri' '{route.Uri}' $KeyVault");
                lines.Add($"{routePrefix}--httpverb' '{route.HttpVerb}' $KeyVault");

                AddAuthenticationConfigLines(route.AuthenticationConfig, routePrefix);

                routeId++;
            }
        }

        private void AddAuthenticationConfigLines(AuthenticationConfig authenticationConfig, string prefix)
        {
            if (authenticationConfig is OidcAuthenticationConfig oidcAuthConfig)
            {
                lines.Add($"{prefix}--authenticationconfig--type' {(int)oidcAuthConfig.Type} $KeyVault");
                lines.Add($"{prefix}--authenticationconfig--uri' '{oidcAuthConfig.Uri}' $KeyVault");
                lines.Add($"{prefix}--authenticationconfig--clientid' '{oidcAuthConfig.ClientId}' $KeyVault");
                lines.Add($"{prefix}--authenticationconfig--clientsecret' '{oidcAuthConfig.ClientSecret}' $KeyVault");
                lines.Add($"{prefix}--authenticationconfig--scopes' '{AddScopes(oidcAuthConfig.Scopes)}' $KeyVault");
            }
        }

        private static string AddScopes(string[] scopes)
        {
            if (scopes == null || scopes.Length == 0)
                return string.Empty;

            return string.Join(',', scopes);
        }
    }
}