using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class KeyVaultProviderTests
    {
        [Fact]
        [IsDev]
        public void ConfigNotEmpty()
        {
            var kvUri = "https://esw-tooling-ci-we.vault.azure.net/";

            var config = new ConfigurationBuilder().AddAzureKeyVault(
                kvUri,
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            //autowire up configs in keyvault to webhooks
            //autowire up configs in keyvault to webhooks
            var section = config.GetSection("event");
            var values = section.GetChildren().ToList();

            var eventHandlerList = new List<EventHandlerConfig>();
            var webhookList = new List<WebhookConfig>(values.Count);
            var endpointList = new Dictionary<string, WebhookConfig>(values.Count);
            foreach (var configurationSection in values)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                eventHandlerList.Add(eventHandlerConfig);

                var path = "webhookconfig";
                if (eventHandlerConfig.WebhookConfig != null)
                {
                    ParseAuthScheme(eventHandlerConfig.WebhookConfig, configurationSection, $"{path}:authenticationconfig");
                    webhookList.Add(eventHandlerConfig.WebhookConfig);
                    AddEndpoints(eventHandlerConfig.WebhookConfig, endpointList, configurationSection, path);
                }

                if (!eventHandlerConfig.CallBackEnabled)
                {
                    continue;
                }

                path = "callbackconfig";
                ParseAuthScheme(eventHandlerConfig.CallbackConfig, configurationSection, $"{path}:authenticationconfig");
                webhookList.Add(eventHandlerConfig.CallbackConfig);
                AddEndpoints(eventHandlerConfig.CallbackConfig, endpointList, configurationSection, path);
            }

            Assert.NotEmpty(eventHandlerList);
            Assert.NotEmpty(webhookList);
            Assert.NotEmpty(endpointList);
        }

        /// <summary>
        /// Creates a list of unique endpoints which are used for authentication sharing and pooling
        /// </summary>
        /// <param name="webhookConfig"></param>
        /// <param name="endpointList"></param>
        /// <param name="configurationSection"></param>
        /// <param name="path"></param>
        private static void AddEndpoints(WebhookConfig webhookConfig, IDictionary<string, WebhookConfig> endpointList, IConfiguration configurationSection, string path)
        {
            //{[event:1:webhookconfig:webhookrequestrules:2:routes:2:authenticationconfig:clientid, tooling.eda.client]}
            //Path = "event:1:webhookconfig:webhookrequestrules:2:routes:1"
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
        private static void ParseAuthScheme(WebhookConfig route, IConfiguration configurationSection, string path)
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
        private static void AddToDictionarySafely(IDictionary<string, WebhookConfig> endpointList, WebhookConfig rule)
        {
            var uri = new Uri(rule.Uri);
            if (!endpointList.ContainsKey(uri.Host.ToLower()))
            {
                endpointList.Add(uri.Host.ToLower(), rule);
            }
        }

        /// <summary>
        /// Hack to parse out the config types, won't be needed after api configuration
        /// </summary>
        /// <param name="configurationSection"></param>
        /// <returns></returns>
        private static OidcAuthenticationConfig ParseOidcAuthenticationConfig(IConfiguration configurationSection)
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
