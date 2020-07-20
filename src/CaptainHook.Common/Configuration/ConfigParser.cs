using CaptainHook.Common.Authentication;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace CaptainHook.Common.Configuration
{
    /// <summary>
    /// <remarks>Won't be needed in V2 has configs are stored in Cosmos and CRD through the API</remarks>
    /// </summary>
    public class ConfigParser
    {
        /// <summary>
        /// Creates a list of unique endpoints which are used for authentication sharing and pooling
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <param name="path"></param>
        public static void AddEndpoints(WebhookConfig webhookConfig, IConfiguration configurationSection, string path)
        {
            if (webhookConfig == null) throw new ArgumentNullException(nameof(webhookConfig));
            if (configurationSection == null) throw new ArgumentNullException(nameof(configurationSection));

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
                    }
                }
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
                oauthAuthenticationConfig.RefreshBeforeInSeconds = 10;
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
