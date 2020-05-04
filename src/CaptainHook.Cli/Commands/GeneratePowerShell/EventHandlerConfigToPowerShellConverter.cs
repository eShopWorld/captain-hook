﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class EventHandlerConfigToPowerShellConverter
    {
        private readonly List<string> lines = new List<string>();

        public async Task<IEnumerable<string>> Convert(IEnumerable<EventHandlerConfig> events)
        {
            int eventId = 1;
            foreach (var eventConfig in events)
            {
                lines.Add($"setConfig 'event--{eventId}--type' '{eventConfig.Type}' $KeyVault");
                lines.Add($"setConfig 'event--{eventId}--name' '{eventConfig.Name}' $KeyVault");

                var webhookPrefix = $"setConfig 'event--{eventId}--webhookconfig";
                AddWebhookDetails(eventConfig.WebhookConfig, webhookPrefix);
                AddAuthenticationConfigLines(eventConfig.WebhookConfig?.AuthenticationConfig, webhookPrefix);
                AddWebHookRules(eventConfig.WebhookConfig?.WebhookRequestRules, webhookPrefix);

                var callbackPrefix = $"setConfig 'event--{eventId}--callbackconfig";
                AddCallbackDetails(eventConfig.CallbackConfig, callbackPrefix);
                AddAuthenticationConfigLines(eventConfig.CallbackConfig?.AuthenticationConfig, callbackPrefix);
                AddWebHookRules(eventConfig.CallbackConfig?.WebhookRequestRules, callbackPrefix);

                eventId++;
            }

            return await Task.FromResult(lines);
        }

        private void AddWebhookDetails(WebhookConfig webhookConfig, string webhookPrefix)
        {
            lines.Add($"{webhookPrefix}--name' '{webhookConfig?.Name}' $KeyVault");
            lines.Add($"{webhookPrefix}--uri' '{webhookConfig?.Uri}' $KeyVault");
            lines.Add($"{webhookPrefix}--httpverb' '{webhookConfig?.HttpVerb}' $KeyVault");
        }

        private void AddCallbackDetails(WebhookConfig callbackConfig, string calbackPrefix)
        {
            lines.Add($"{calbackPrefix}--name' '{callbackConfig?.Name}' $KeyVault");
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

                lines.Add($"{rulePrefix}--source--path' '{rule.Source.Path}' $KeyVault");
                lines.Add($"{rulePrefix}--source--type' '{rule.Source.Type}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--path' '{rule.Destination.Path}' $KeyVault");
                lines.Add($"{rulePrefix}--destination--type' '{rule.Destination.Type}' $KeyVault");
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
                lines.Add($"{prefix}--authenticationconfig--scopes' '{string.Join(',', oidcAuthConfig.Scopes)}' $KeyVault");
            }
        }
    }
}