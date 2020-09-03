using System;
using System.Collections.Generic;
using System.Linq;
using Azure.Identity;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class KeyVaultProviderTests
    {
        [Fact(Skip = "Superseded by ConfigLoaderTests.ConfigNotEmpty")]
        [IsDev]
        public void ConfigNotEmpty()
        {
            var kvUri = "https://esw-tooling-ci-we.vault.azure.net/";

            var config = new ConfigurationBuilder().AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential()).Build();

            //autowire up configs in keyvault to webhooks
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

                foreach(var subscriber in eventHandlerConfig.AllSubscribers)
                {
                    var path = "webhookconfig";
                    ConfigParser.ParseAuthScheme(subscriber, configurationSection, $"{path}:authenticationconfig");
                    webhookList.Add(subscriber);
                    ConfigParser.AddEndpoints(subscriber, configurationSection, path);

                    if (subscriber.Callback != null)
                    {
                        path = "callbackconfig";
                        ConfigParser.ParseAuthScheme(subscriber.Callback, configurationSection, $"{path}:authenticationconfig");
                        webhookList.Add(subscriber.Callback);
                        ConfigParser.AddEndpoints(subscriber.Callback, configurationSection, path);
                    }
                }
            }

            Assert.NotEmpty(eventHandlerList);
            Assert.NotEmpty(webhookList);
        }
    }
}
