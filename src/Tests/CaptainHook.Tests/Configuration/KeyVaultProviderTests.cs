using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor;
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
                    ConfigParser.ParseAuthScheme(eventHandlerConfig.WebhookConfig, configurationSection, $"{path}:authenticationconfig");
                    webhookList.Add(eventHandlerConfig.WebhookConfig);
                    ConfigParser.AddEndpoints(eventHandlerConfig.WebhookConfig, endpointList, configurationSection, path);
                }

                if (!eventHandlerConfig.CallBackEnabled)
                {
                    continue;
                }

                path = "callbackconfig";
                ConfigParser.ParseAuthScheme(eventHandlerConfig.CallbackConfig, configurationSection, $"{path}:authenticationconfig");
                webhookList.Add(eventHandlerConfig.CallbackConfig);
                ConfigParser.AddEndpoints(eventHandlerConfig.CallbackConfig, endpointList, configurationSection, path);
            }

            Assert.NotEmpty(eventHandlerList);
            Assert.NotEmpty(webhookList);
            Assert.NotEmpty(endpointList);
        }
    }
}
