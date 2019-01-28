using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
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
        [Fact(Skip = "Work in progress needs infra and refactor")]
        public async Task BuildConfigurationHappyPath()
        {
            var kvUri = "https://dg-test.vault.azure.net/";

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

                //take the parameters from the payload of the message and then add them to the requests which are sent to the webhook and callback

                if (eventHandlerConfig.Name == "goc-checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent")
                {
                    eventHandlerConfig.EventParsers = new List<EventParser>
                    {
                        new EventParser
                        {
                            ActionPreformedOn = ActionPreformedOn.Message,
                            Name = "OrderCodeParser",
                            Source = new ParserLocation
                            {
                                //take it from the body of the message
                                Name = "OrderCode",
                                QueryLocation = QueryLocation.Body
                            },
                            Destination = new ParserLocation
                            {
                                //put it in the URI
                                Name = "OrderCode",
                                QueryLocation = QueryLocation.Uri
                            }
                        },
                        new EventParser
                        {
                            ActionPreformedOn = ActionPreformedOn.Callback,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                QueryLocation = QueryLocation.Body
                            },
                            Destination = new ParserLocation
                            {
                                QueryLocation = QueryLocation.Uri
                            }
                        },
                        new EventParser
                        {
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderConfirmationRequestDto",
                                QueryLocation = QueryLocation.Body
                            },
                            Destination = new ParserLocation
                            {
                                QueryLocation = QueryLocation.Body
                            }
                        }
                    };
                }

                if (eventHandlerConfig.Name == "goc-checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
                {
                    eventHandlerConfig.EventParsers = new List<EventParser>
                    {
                        new EventParser
                        {
                            Name = "OrderCode",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                QueryLocation = QueryLocation.Body
                            },
                            Destination = new ParserLocation
                            {
                                QueryLocation = QueryLocation.Uri
                            }
                        },
                        new EventParser
                        {
                            Name = "Payload Parser from event to webhook",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "PreOrderApiInternalModelOrderRequestDto",
                                QueryLocation = QueryLocation.Body
                            },
                            Destination = new ParserLocation
                            {
                                QueryLocation = QueryLocation.Body
                            }
                        }
                    };
                }

                //todo dup check on webhook names/urls
                eventList.Add(eventHandlerConfig);
                webhookList.Add(webHookConfig);
            }
        }
    }
}
