using System;
using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Nasty;
using Eshopworld.Tests.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Newtonsoft.Json;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class Test
    {
        [IsLayer0]
        [Fact]
        public void TestTest()
        {
            var payload = new HttpResponseDto
            {
                OrderCode = Guid.NewGuid(),
                Content = string.Empty,
                StatusCode = 200
            };
            JsonConvert.SerializeObject(payload);
        }
    }


    public class ConfigurationBuilderTests
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
               new object[]
               {
                   new EventHandlerConfig
                   {
                       Name = "Event 1",
                       Type = "blahblah",
                       WebHookConfig = new WebhookConfig
                       {
                           Name = "Webhook1",
                           HttpVerb = "POST",
                           Uri = "https://blah.blah.eshopworld.com",
                           AuthenticationConfig = new OidcAuthenticationConfig
                           {
                               Type = AuthenticationType.OIDC,
                               Uri = "https://blah-blah.sts.eshopworld.com",
                               ClientId = "ClientId",
                               ClientSecret = "ClientSecret",
                               Scopes = new []{"scope1", "scope2"}
                           },
                           WebhookQueryRules = new List<WebhookQueryRule>
                           {
                               new WebhookQueryRule
                               {
                                   Name = "OrderCode",
                                   Source = new ParserLocation
                                   {
                                       Location = Location.PayloadBody,
                                       Path = "OrderCode"
                                   },
                                   Destination = new ParserLocation
                                   {
                                       Location = Location.Uri
                                   }
                               },
                               new WebhookQueryRule
                               {
                                   Type = QueryRuleTypes.WebHook,
                                   Source = new ParserLocation
                                   {
                                       Path = "BrandType",
                                       Location = Location.PayloadBody
                                   },
                                   Routes = new List<WebhookConfigRoutes>
                                   {
                                       new WebhookConfigRoutes
                                       {
                                           Uri = "https://blah.blah.brandytype.eshopworld.com",
                                           HttpVerb = "POST",
                                           Selector = "Brand1",
                                           AuthenticationConfig = new AuthenticationConfig
                                           {
                                               Type = AuthenticationType.None
                                           }
                                       }
                                   }
                               },
                               new WebhookQueryRule
                               {
                                   Source = new ParserLocation
                                   {
                                       Path = "OrderConfirmationRequestDto",
                                       Location = Location.PayloadBody
                                   },
                                   Type = QueryRuleTypes.Model,
                                   Destination = new ParserLocation
                                   {
                                       Location = Location.PayloadBody
                                   }
                               }
                           }
                       },
                       CallbackConfig = new WebhookConfig
                       {
                           HttpVerb = "PUT",
                           Uri = "https://callback.eshopworld.com",
                           AuthenticationConfig = new AuthenticationConfig
                           {
                               Type = AuthenticationType.None
                           },
                           WebhookQueryRules = new List<WebhookQueryRule>
                           {
                               new WebhookQueryRule
                               {
                                   Type = QueryRuleTypes.Parameter,
                                   Source = new ParserLocation
                                   {
                                       Location = Location.MessageBody,
                                       Path = "OrderCode"
                                   },
                                   Destination = new ParserLocation
                                   {
                                       Location = Location.PayloadBody,
                                       Path = "OrderCode"
                                   }
                               },
                               new WebhookQueryRule
                               {
                                   Type = QueryRuleTypes.Parameter,
                                   Source = new ParserLocation
                                   {
                                       Location = Location.StatusCode,
                                   },
                                   Destination = new ParserLocation
                                   {
                                       Location = Location.PayloadBody,
                                       Path = "StatusCode"
                                   }
                                   
                               }
                           }
                       }
                   }
               }
            };


        [IsLayer1]
        [Fact(Skip = "Work in progress needs infra and refactor")]
        public void BuildConfigurationHappyPath()
        {
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
                    eventHandlerConfig.EventParsers = new List<WebhookQueryRule>
                    {
                        new WebhookQueryRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Message,
                            Name = "OrderCodeParser",
                            Source = new ParserLocation
                            {
                                //take it from the body of the message
                                Name = "OrderCode",
                                Location = Location.PayloadBody
                            },
                            Destination = new ParserLocation
                            {
                                //put it in the URI
                                Name = "OrderCode",
                                Location = Location.Uri
                            }
                        },
                        new WebhookQueryRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Callback,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                Location = Location.PayloadBody
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Uri
                            }
                        },
                        new WebhookQueryRule
                        {
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderConfirmationRequestDto",
                                Location = Location.PayloadBody
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.PayloadBody
                            }
                        }
                    };
                }

                if (eventHandlerConfig.Name == "goc-checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
                {
                    eventHandlerConfig.EventParsers = new List<WebhookQueryRule>
                    {
                        new WebhookQueryRule
                        {
                            Name = "OrderCode",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "OrderCode",
                                Location = Location.PayloadBody
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.Uri
                            }
                        },
                        new WebhookQueryRule
                        {
                            Name = "Payload Parser from event to webhook",
                            ActionPreformedOn = ActionPreformedOn.Webhook,
                            Source = new ParserLocation
                            {
                                Name = "PreOrderApiInternalModelOrderRequestDto",
                                Location = Location.PayloadBody
                            },
                            Destination = new ParserLocation
                            {
                                Location = Location.PayloadBody
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
