using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Microsoft.Extensions.Configuration;

namespace CaptainHook.EventHandlerActor
{
    /// <summary>
    /// <remarks>Won't be needed in V2 has configs are stored in Cosmos and CRD through the API</remarks>
    /// </summary>
    public class ConfigParser
    {
        /// <summary>
        /// Creates a list of unique endpoints which are used for authentication sharing and pooling
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="endpointList"></param>
        /// <param name="configurationSection"></param>
        /// <param name="path"></param>
        public static void AddEndpoints(WebhookConfig webhookConfig, IDictionary<string, WebhookConfig> endpointList, IConfiguration configurationSection, string path)
        {
            //creates a list of endpoints so they can be shared for authentication and http pooling
            if (string.IsNullOrWhiteSpace(webhookConfig.Uri))
            {
                if (!webhookConfig.WebhookRequestRules.Any(r => r.Routes.Any()))
                {
                    return;
                }

                for (var i = 0; i < webhookConfig.WebhookRequestRules.Count; i++)
                {
                    var webhookRequestRule = webhookConfig.WebhookRequestRules[i];
                    for (var y = 0; y < webhookRequestRule.Routes.Count; y++)
                    {
                        var route = webhookRequestRule.Routes[y];
                        if (string.IsNullOrWhiteSpace(route.Uri))
                        {
                            continue;
                        }

                        var authPath = $"{path}:webhookrequestrules:{i + 1}:routes:{y + 1}:authenticationconfig";
                        ParseAuthScheme(route, configurationSection, authPath);
                        AddToDictionarySafely(endpointList, route);
                    }
                }
            }
            else
            {
                AddToDictionarySafely(endpointList, webhookConfig);
            }
        }

        /// <summary>
        /// Parse the auth scheme from config to concrete type
        /// </summary>
        /// <param name="route"></param>
        /// <param name="configurationSection"></param>
        /// <param name="path"></param>
        public static void ParseAuthScheme(WebhookConfig route, IConfiguration configurationSection, string path)
        {
            if (route.AuthenticationConfig.Type == AuthenticationType.Basic)
            {
                var basicAuthenticationConfig = new BasicAuthenticationConfig
                {
                    Username = configurationSection[path + ":username"],
                    Password = configurationSection[path + ":password"]
                };
                route.AuthenticationConfig = basicAuthenticationConfig;
            }

            if (route.AuthenticationConfig.Type == AuthenticationType.OIDC)
            {
                route.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection(path));
            }

            if (route.AuthenticationConfig.Type != AuthenticationType.Custom)
            {
                return;
            }

            route.AuthenticationConfig = ParseOidcAuthenticationConfig(configurationSection.GetSection(path));
            route.AuthenticationConfig.Type = AuthenticationType.Custom;
        }

        /// <summary>
        /// Safe adds to the dictionary if it does not already exist
        /// </summary>
        /// <param name="endpointList"></param>
        /// <param name="rule"></param>
        public static void AddToDictionarySafely(IDictionary<string, WebhookConfig> endpointList, WebhookConfig rule)
        {
            var uri = new Uri(rule.Uri);
            if (!endpointList.ContainsKey(uri.Host.ToLowerInvariant()))
            {
                endpointList.Add(uri.Host.ToLowerInvariant(), rule);
            }
        }

        /// <summary>
        /// Hack to parse out the config types, won't be needed after api configuration
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        public static OidcAuthenticationConfig ParseOidcAuthenticationConfig(IConfiguration configurationSection)
        {
            var oauthAuthenticationConfig = new OidcAuthenticationConfig
            {
                ClientId = configurationSection["clientid"],
                ClientSecret = configurationSection["clientsecret"],
                Uri = configurationSection["uri"],
                Scopes = configurationSection["scopes"].Split(" ")
            };

            var refresh = configurationSection["refresh"];
            if (string.IsNullOrWhiteSpace(refresh))
            {
                oauthAuthenticationConfig.RefreshBeforeInSeconds = 50;
            }
            else
            {
                if (int.TryParse(refresh, out var refreshValue))
                {
                    oauthAuthenticationConfig.RefreshBeforeInSeconds = refreshValue;
                }
            }

            return oauthAuthenticationConfig;
        }
    }
}
