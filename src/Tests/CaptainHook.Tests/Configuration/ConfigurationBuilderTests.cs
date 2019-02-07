﻿using System.Collections.Generic;
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
   public class ConfigurationBuilderTests
    {
        [IsLayer1]
        [Fact(Skip = "Work in progress needs infra and refactor")]
        public void BuildConfigurationHappyPath()
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

                if (eventHandlerConfig.Name == "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent")
                {
                    eventHandlerConfig.EventParsers = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Message,
                            Name = "OrderCodeParser",
                            Source = new ParserLocation
                            {
                                //take it from the body of the message
                                Name = "OrderCode",
                                Location = Location.Body
                            },
                            Destination = new ParserLocation
                            {
                                //put it in the URI
                                Name = "OrderCode",
                                Location = Location.Uri
                            }
                        },
                        new WebhookRequestRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Callback,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                Location = Location.Body
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Uri
                            }
                        },
                        new WebhookRequestRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderConfirmationRequestDto",
                                Location = Location.Body
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Body
                            }
                        }
                    };
                }

                if (eventHandlerConfig.Name == "goc-checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
                {
                    eventHandlerConfig.EventParsers = new List<WebhookRequestRule>
                    {
                        new WebhookRequestRule
                        {
                            Name = "OrderCode",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                Location = Location.Body
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Uri
                            }
                        },
                        new WebhookRequestRule
                        {
                            Name = "Payload Parser from event to webhook",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "PreOrderApiInternalModelOrderRequestDto",
                                Location = Location.Body
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Body
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
