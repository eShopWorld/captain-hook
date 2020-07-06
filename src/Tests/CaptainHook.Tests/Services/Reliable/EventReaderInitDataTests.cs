using System;
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
        private readonly WebhookConfig _webhookConfig;

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

            _webhookConfig = new WebhookConfig();
        }

        [Fact]
        [IsUnit]
        public void CanPassSubscriberConfiguration()
        {
            var buffer = EventReaderInitData
                .FromSubscriberConfiguration(_subscriberConfiguration, _webhookConfig)
                .ToByteArray();

            var eventReaderInitData = EventReaderInitData.FromByteArray(buffer);

            eventReaderInitData.SubscriberName.Should().Be(_subscriberConfiguration.SubscriberName);
            eventReaderInitData.EventType.Should().Be(_subscriberConfiguration.EventType);
            eventReaderInitData.SubscriberConfiguration.Should().BeEquivalentTo(_subscriberConfiguration);
        }

        [Fact, IsUnit]
        public void FromSubscriberConfiguration_HeartBeatIntervalInSeconds_MapsToTimeSpan()
        {
            // Arrange
            var heartBeatDisabledSubscriber = new SubscriberConfiguration
            {
                HeartBeatInterval = "00:00:05"
            };

            // Act
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber, _webhookConfig);

            // Assert
            initData.HeartBeatInterval.Should().Be(TimeSpan.FromSeconds(5.0));
        }

        [Fact, IsUnit]
        public void FromSubscriberConfiguration_HeartBeatIntervalInMinutes_MapsToTimeSpan()
        {
            // Arrange
            var heartBeatDisabledSubscriber = new SubscriberConfiguration
            {
                HeartBeatInterval = "00:06:00"
            };

            // Act
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber, _webhookConfig);

            // Assert
            initData.HeartBeatInterval.Should().Be(TimeSpan.FromMinutes(6.0));
        }

        [Fact, IsUnit]
        public void FromSubscriberConfiguration_HeartBeatIntervalUnavailable_MapsToNullTimeSpan()
        {
            // Arrange
            var heartBeatDisabledSubscriber = new SubscriberConfiguration();

            // Act
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber, _webhookConfig);

            // Assert
            initData.HeartBeatInterval.Should().BeNull();
        }
    }
}
