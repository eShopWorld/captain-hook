using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using Eshopworld.Tests.Core;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;

namespace CaptainHook.Tests.Services.Reliable
{
    public class EventReaderInitDataTests
    {
        private readonly SubscriberConfiguration _subscriberConfiguration;

        public EventReaderInitDataTests()
        {
            _subscriberConfiguration = new SubscriberConfiguration
            {
                SubscriberName = "subA",
                EventType = "test.type",
                Uri = "test-uri-1",
                Name = "test.type",
                Callback = new WebhookConfig
                {
                    AuthenticationConfig = new OidcAuthenticationConfig
                    {
                        Scopes = new string[] { "scope1", "scope2" },
                        ClientSecret = "verylongsecret",
                        ClientId = "aclientid",
                        Uri = "StsUri"
                    }
                },
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "ActivityConfirmationRequestDto",
                            Type = DataType.Model
                        },
                        Destination = new ParserLocation
                        {
                            Type = DataType.Model
                        }
                    },
                    new WebhookRequestRule
                    {
                        Source = new ParserLocation
                        {
                            Path = "TenantCode"
                        },
                        Destination = new ParserLocation
                        {
                            RuleAction = RuleAction.Route
                        },
                        Routes = new List<WebhookConfigRoute>
                        {
                            new WebhookConfigRoute
                            {
                                Uri = "http://testuri1",
                                Selector = "test-selector1",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Scopes = new string[] { "scope1", "scope2" },
                                    ClientSecret = "verylongsecret",
                                    ClientId = "aclientid",
                                    Uri = "StsUri"
                                }
                            },
                            new WebhookConfigRoute
                            {
                                Uri = "http://testuri2",
                                Selector = "test-selector2",
                                AuthenticationConfig = new OidcAuthenticationConfig
                                {
                                    Scopes = new string[] { "scope1", "scope2" },
                                    ClientSecret = "verylongsecret",
                                    ClientId = "aclientid",
                                    Uri = "StsUri"
                                }
                            }
                        }
                    }
                }
            };
        }

        [Fact]
        [IsLayer0]
        public void CanPassSubscriberConfiguration()
        {
            var buffer = EventReaderInitData
                .FromSubscriberConfiguration(_subscriberConfiguration)
                .ToByteArray();

            var eventReaderInitData = EventReaderInitData.FromByteArray(buffer);

            eventReaderInitData.SubscriberName.Should().Be(_subscriberConfiguration.SubscriberName);
            eventReaderInitData.EventType.Should().Be(_subscriberConfiguration.EventType);
            eventReaderInitData.SubscriberConfiguration.Should().BeEquivalentTo(_subscriberConfiguration);
        }
    }
}
