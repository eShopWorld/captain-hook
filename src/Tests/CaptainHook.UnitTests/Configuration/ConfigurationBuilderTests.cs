using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common;
using Eshopworld.Tests.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Xunit;

namespace CaptainHook.UnitTests.Configuration
{
    public class ConfigurationBuilderTests
    {
        [IsLayer1]
        [Fact]
        public async Task BuildConfigurationHappyPath()
        {
            var kvUri = "https://dg-testdg.vault.azure.net/";

            var config = new ConfigurationBuilder().AddAzureKeyVault(
                kvUri,
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            //autowire up configs in keyvault to webhooks
            var section = config.GetSection("event");
            var values = section.GetChildren().ToList();

            var eventList = new List<EventHandlerConfig>(values.Count);
            var webhookList = new List<WebhookConfig>(values.Count);

            foreach (var configurationSection in values)
            {
                var eventHandlerConfig = configurationSection.Get<EventHandlerConfig>();
                var webHookConfig = configurationSection.GetSection($"webhook:{configurationSection.Key}").Get<WebhookConfig>();

                eventHandlerConfig.EventParsers = new List<EventParser>
                {
                    new EventParser
                    {
                        Source = new ParserLocation
                        {
                            Name = "OrderCode",
                            QueryLocation = QueryLocation.Body
                        },
                        Destination = new ParserLocation
                        {
                            QueryLocation = QueryLocation.Uri
                        }
                    }
                };


                //if (configurationSection.Key == "goc")
                //{
                //    var event0 = new EventParsers
                //    {
                //        Name = "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent",
                //        ModelQueryPath = "OrderConfirmationRequestDto"
                //    };
                //    webHookConfig.EventParsers.Add(event0);

                //    var event1 = new EventParsers
                //    {
                //        Name = "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent",
                //        ModelQueryPath = "PreOrderApiInternalModelOrderRequestDto"
                //    };
                //    webHookConfig.EventParsers.Add(event1);
                //}

                //todo dup check on webhook names/urls
                eventList.Add(eventHandlerConfig);
                webhookList.Add(webHookConfig);
            }
        }
    }
}
