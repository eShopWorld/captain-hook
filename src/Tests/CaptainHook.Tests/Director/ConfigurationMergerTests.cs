using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.Domain.Models;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ConfigurationMergerTests
    {
        [Fact, IsLayer0]
        public void OnlyKVSubscribers_AllInResult()
        {
            var kvSubscribers = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var result = new ConfigurationMerger().Merge(kvSubscribers, new List<Subscriber>());

            using (new AssertionScope())
            {
                result.Should().HaveCount(3);
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
                result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
            }
        }

        [Fact, IsLayer0]
        public void OnlyCosmosSubscribers_AllInResult()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST").Create(),
                new SubscriberBuilder().WithEvent("testevent.completed").WithWebhook("https://cosmos.eshopworld.com/testevent-completed/", "POST").Create(),
                new SubscriberBuilder().WithEvent("testevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/testevent2/", "POST").Create(),
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(3);
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
                result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
            }
        }

        [Fact, IsLayer0]
        public void WhenSameEventsExistInKvSubscribersAndCosmosSubscribers_CosmosSubscribersMustOverrideKvSubscribers()
        {
            var kvSubscribers = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testEVENT").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("TESTevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST").Create(),
                new SubscriberBuilder().WithEvent("newtestevent.completed").WithWebhook("https://cosmos.eshopworld.com/newtestevent-completed/", "POST").Create(),
                new SubscriberBuilder().WithEvent("newtestevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/newtestevent2/", "POST").Create(),
            };

            var result = new ConfigurationMerger().Merge(kvSubscribers, cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(5);

                result.Should().Contain(x => x.Name == "TESTevent" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/testevent/");

                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://blah.blah.eshopworld.com");
                result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://blah.blah.eshopworld.com");

                result.Should().Contain(x => x.Name == "newtestevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://cosmos.eshopworld.com/newtestevent2/");
                result.Should().Contain(x => x.Name == "newtestevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/newtestevent-completed/");
            }
        }

        [Fact, IsLayer0]
        public void CosmosSubscriberWithAuthentication_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST",
                        new Authentication{  Uri = "https://blah-blah.sts.eshopworld.com", ClientId = "captain-hook-id", Secret = "verylongpassword", }
                    ).Create(),
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                result.Should().Contain(x => x.Name == "testevent"
                                             && x.SubscriberName == "captain-hook"
                                             && IsOidcAuthentication(x, "captain-hook-id", "verylongpassword", "https://blah-blah.sts.eshopworld.com"));
            }
        }

        private bool IsOidcAuthentication(SubscriberConfiguration sc, string clientId, string secret, string uri)
        {
            var oidcAuth = sc.AuthenticationConfig as OidcAuthenticationConfig;
            if (oidcAuth == null)
            {
                return false;
            }

            return oidcAuth.ClientId == clientId && oidcAuth.ClientSecret == secret && oidcAuth.Uri == uri;
        }

        [Fact(Skip = "Callback handling not needed as for now"), IsLayer0]
        public void CosmosSubscriberWithCallback_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST")
                    .WithCallback("https://cosmos.eshopworld.com/callback/", "PUT")
                    .Create(),
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                result.Should().Contain(x => x.Callback.Name == "captain-hook"
                                             && x.Callback.EventType == "testevent"
                                             && x.Callback.Uri == "https://cosmos.eshopworld.com/callback/" && x.Callback.HttpVerb == "PUT");
            }
        }

        [Fact(Skip = "DQL handling not needed as for now"), IsLayer0]
        public void CosmosSubscriberWithDlq_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent-dlq").WithName("DLQ")
                    .WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST")
                    .WithDlq("https://cosmos.eshopworld.com/dlq/", "PUT")
                    .Create(),
            };

            var result = new ConfigurationMerger().Merge(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(2);
                result.Should().Contain(x => x.Name == "testevent-dlq"
                                             && x.SubscriberName == "DLQ"
                                             && x.WebhookRequestRules.SelectMany(w => w.Routes).All(r => r.Uri == "https://cosmos.eshopworld.com/dlq/"));
            }
        }
    }
}