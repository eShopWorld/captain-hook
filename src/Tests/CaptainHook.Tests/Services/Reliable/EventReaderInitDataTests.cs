using System;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.ServiceModels;
using Eshopworld.Tests.Core;
using FluentAssertions;
using System.Collections.Generic;
using Xunit;
using FluentAssertions.Execution;

namespace CaptainHook.Tests.Services.Reliable
{
    public class EventReaderInitDataTests
    {
        private readonly SubscriberConfiguration _subscriberConfiguration;
        private readonly SubscriberConfiguration _invalidSubscriberConfiguration;

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
                        Source = new SourceParserLocation
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
                        Source = new SourceParserLocation
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

            _invalidSubscriberConfiguration = new SubscriberConfiguration
            {
                SubscriberName = "subA",
                EventType = "test.type",
                Uri = "test-uri-1",
                Name = "test.type",
                AuthenticationConfig = null
            };
        }

        [Fact]
        [IsUnit]
        public void CanPassSubscriberConfiguration()
        {
            var buffer = EventReaderInitData.FromSubscriberConfiguration(_subscriberConfiguration).ToByteArray();

            var eventReaderInitData = EventReaderInitData.FromByteArray(buffer);

            eventReaderInitData.SubscriberName.Should().Be(_subscriberConfiguration.SubscriberName);
            eventReaderInitData.EventType.Should().Be(_subscriberConfiguration.EventType);
            eventReaderInitData.SubscriberConfiguration.Should().BeEquivalentTo(_subscriberConfiguration);
            eventReaderInitData.MaxDeliveryCount.Should().Be(_subscriberConfiguration.MaxDeliveryCount);
        }

        [Fact]
        [IsUnit]
        public void CanPassAuthDataConfiguration()
        {
            var buffer = EventReaderInitData.FromSubscriberConfiguration(_subscriberConfiguration).ToByteArray();

            var eventReaderInitData = EventReaderInitData.FromByteArray(buffer);

            using (new AssertionScope())
            {
                ValidateAuthConfig(eventReaderInitData.SubscriberConfiguration.Callback.AuthenticationConfig);
                ValidateAuthConfig(eventReaderInitData.SubscriberConfiguration.WebhookRequestRules[1].Routes[0].AuthenticationConfig);
                ValidateAuthConfig(eventReaderInitData.SubscriberConfiguration.WebhookRequestRules[1].Routes[1].AuthenticationConfig);
            }
        }

        [Fact]
        [IsUnit]
        public void InvalidAuthDataTranslatesToNoAuthentication()
        {
            var buffer = EventReaderInitData.FromSubscriberConfiguration(_invalidSubscriberConfiguration).ToByteArray();

            var eventReaderInitData = EventReaderInitData.FromByteArray(buffer);

            using (new AssertionScope())
            {
                eventReaderInitData.SubscriberConfiguration.AuthenticationConfig.Should().BeOfType(typeof(AuthenticationConfig));
                eventReaderInitData.SubscriberConfiguration.AuthenticationConfig.Type.Should().Be(AuthenticationType.None);
            }
        }

        private void ValidateAuthConfig(AuthenticationConfig authConfig)
        {
            authConfig.Should().BeOfType(typeof(OidcAuthenticationConfig));
            
            var oidcAuthConfig = (OidcAuthenticationConfig)authConfig;
            oidcAuthConfig.Scopes.Should().Contain(new string[] { "scope1", "scope2" });
            oidcAuthConfig.ClientSecret.Should().Be("verylongsecret");
            oidcAuthConfig.ClientId.Should().Be("aclientid");
            oidcAuthConfig.Uri.Should().Be("StsUri");
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
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber);

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
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber);

            // Assert
            initData.HeartBeatInterval.Should().Be(TimeSpan.FromMinutes(6.0));
        }

        [Fact, IsUnit]
        public void FromSubscriberConfiguration_HeartBeatIntervalUnavailable_MapsToNullTimeSpan()
        {
            // Arrange
            var heartBeatDisabledSubscriber = new SubscriberConfiguration();

            // Act
            var initData = EventReaderInitData.FromSubscriberConfiguration(heartBeatDisabledSubscriber);

            // Assert
            initData.HeartBeatInterval.Should().BeNull();
        }
    }
}
