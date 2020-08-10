using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Services
{
    public class MessageDataDataSerializationTests
    {
        [Fact, IsUnit]
        public void DataContractSerializer_ShouldSerialize_MessageData()
        {
            var webhookConfig = new WebhookConfig
            {
                Name = "test-name-1",
                AuthenticationConfig = new BasicAuthenticationConfig
                {
                    Type = AuthenticationType.Basic,
                    Password = "secret",
                    Username = "username",
                },
                Uri = "https://test.com/webhook"
            };

            var subscriberConfiguration = new SubscriberConfiguration
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
                                AuthenticationConfig = new AuthenticationConfig()
                                {
                                    Type = AuthenticationType.Custom,
                                }
                            }
                        }
                    }
                }
            };

            var data = new MessageData("payload", "type", "subscriberName", "replyTo", true);
            data.SubscriberConfig = subscriberConfiguration;

            AssertMessageDataCanBeSerialized(data);
        }

        private static void AssertMessageDataCanBeSerialized(MessageData data)
        {
            var ser = new DataContractSerializer(typeof(MessageData));
            byte[] result;
            using (var ms = new MemoryStream())
            {
                ser.WriteObject(ms, data);
                result = ms.ToArray();
            }

            result.Should().NotBeNullOrEmpty();
        }
    }
}
