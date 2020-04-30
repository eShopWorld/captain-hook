using System.Collections.Generic;
using System.Linq;
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

                foreach(var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    var path = "webhookconfig";
                    ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                    webhookList.Add(subscriber);
                    ConfigParser.AddEndpoints(subscriber, endpointList, configurationSection, path);

                    if (subscriber.Callback != null)
                    {
                        path = "callbackconfig";
                        ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                        webhookList.Add(subscriber.Callback);
                        ConfigParser.AddEndpoints(subscriber.Callback, endpointList, configurationSection, path);
                    }
                }
            }

            Assert.NotEmpty(eventHandlerList);
            Assert.NotEmpty(webhookList);
            Assert.NotEmpty(endpointList);
        }
    }
}
