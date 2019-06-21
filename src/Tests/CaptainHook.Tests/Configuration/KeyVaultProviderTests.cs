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
            var kvUri = "https://dgtest.vault.azure.net/";

            var config = new ConfigurationBuilder().AddAzureKeyVault(
                kvUri,
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            //autowire up configs in keyvault to webhooks
            var section = config.GetSection("event");
            var values = section.GetChildren().ToList();

            var eventHandlerList = new List<EventHandlerConfig>();
            var webhookList = new List<WebhookConfig>(values.Count);

            foreach (var configurationSection in values)
            {
                //temp work around until config comes in through the API
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                eventHandlerList.Add(eventHandlerConfig);

                if (eventHandlerConfig.WebhookConfig != null)
                {
                    if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.Basic)
                    {
                        var basicAuthenticationConfig = new BasicAuthenticationConfig
                        {
                            Username = configurationSection["webhookconfig:authenticationconfig:username"],
                            Password = configurationSection["webhookconfig:authenticationconfig:password"]
                        };
                        eventHandlerConfig.WebhookConfig.AuthenticationConfig = basicAuthenticationConfig;
                    }

                    if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.OIDC)
                    {
                        eventHandlerConfig.WebhookConfig.AuthenticationConfig =
                            ParseOidcAuthenticationConfig(
                                configurationSection.GetSection("webhookconfig:authenticationconfig"));
                    }

                    if (eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                    {
                        eventHandlerConfig.WebhookConfig.AuthenticationConfig =
                            ParseOidcAuthenticationConfig(
                                configurationSection.GetSection("webhookconfig:authenticationconfig"));
                        eventHandlerConfig.WebhookConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                    }

                    webhookList.Add(eventHandlerConfig.WebhookConfig);
                }

                if (eventHandlerConfig.CallBackEnabled)
                {
                    if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.Basic)
                    {
                        var basicAuthenticationConfig = new BasicAuthenticationConfig
                        {
                            Username = configurationSection["webhookconfig:authenticationconfig:username"],
                            Password = configurationSection["webhookconfig:authenticationconfig:password"]
                        };
                        eventHandlerConfig.CallbackConfig.AuthenticationConfig = basicAuthenticationConfig;
                    }

                    if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.OIDC)
                    {
                        eventHandlerConfig.CallbackConfig.AuthenticationConfig =
                            ParseOidcAuthenticationConfig(
                                configurationSection.GetSection("callbackconfig:authenticationconfig"));
                    }

                    if (eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type == AuthenticationType.Custom)
                    {
                        eventHandlerConfig.CallbackConfig.AuthenticationConfig =
                            ParseOidcAuthenticationConfig(
                                configurationSection.GetSection("callbackconfig:authenticationconfig"));
                        eventHandlerConfig.CallbackConfig.AuthenticationConfig.Type = AuthenticationType.Custom;
                    }

                    webhookList.Add(eventHandlerConfig.CallbackConfig);
                }
            }

            Assert.NotEmpty(eventHandlerList);
            Assert.NotEmpty(webhookList);
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
