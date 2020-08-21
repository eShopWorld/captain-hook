using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.Domain.Entities;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class ConfigurationMergerTests
    {
        [Fact, IsUnit]
        public async Task OnlyKVSubscribers_AllInResult()
        {
            var kvSubscribers = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(Mock.Of<ISecretProvider>()));
            var result = await configurationMerger.MergeAsync(kvSubscribers, new List<SubscriberEntity>());

            using (new AssertionScope())
            {
                result.Should().HaveCount(3);
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
                result.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
                result.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
            }
        }

        [Fact, IsUnit]
        public async Task OnlyCosmosSubscribers_AllInResult()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "selector").Create(),
                new SubscriberBuilder().WithEvent("testevent.completed").WithWebhook("https://cosmos.eshopworld.com/testevent-completed/", "POST", "selector").Create(),
                new SubscriberBuilder().WithEvent("testevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/testevent2/", "POST", "selector").Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(Mock.Of<ISecretProvider>()));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(3);
                result.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "captain-hook");
                result.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "subscriber1");
                result.Should().Contain(x => x.EventType == "testevent.completed" && x.SubscriberName == "captain-hook");
            }
        }

        [Fact, IsUnit]
        public async Task WhenSameEventsExistInKvSubscribersAndCosmosSubscribers_CosmosSubscribersMustOverrideKvSubscribers()
        {
            var kvSubscribers = new[]
            {
                new SubscriberConfigurationBuilder().WithType("testEVENT").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithType("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithType("testevent.completed").Create(),
            };

            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("TESTevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "selector").Create(),
                new SubscriberBuilder().WithEvent("newtestevent.completed").WithWebhook("https://cosmos.eshopworld.com/newtestevent-completed/", "POST", "selector").Create(),
                new SubscriberBuilder().WithEvent("newtestevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/newtestevent2/", "POST", "selector").Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(Mock.Of<ISecretProvider>()));
            var result = await configurationMerger.MergeAsync(kvSubscribers, cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(5);

                result.Should().Contain(x => x.EventType == "TESTevent" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/testevent/");

                result.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://blah.blah.eshopworld.com");
                result.Should().Contain(x => x.EventType == "testevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://blah.blah.eshopworld.com");

                result.Should().Contain(x => x.EventType == "newtestevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://cosmos.eshopworld.com/newtestevent2/");
                result.Should().Contain(x => x.EventType == "newtestevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/newtestevent-completed/");
            }
        }

        [Fact, IsUnit]
        public async Task CosmosSubscriberWithAuthentication_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder()
                    .WithEvent("testevent")
                    .WithWebhook(
                        "https://cosmos.eshopworld.com/testevent/",
                        "POST",
                        "selector",
                        authentication: new OidcAuthenticationEntity(
                            "captain-hook-id",
                            "kv-secret-name",
                            "https://blah-blah.sts.eshopworld.com",
                            new[] { "scope1" })
                    ).Create(),
            };

            var mock = new Mock<ISecretProvider>();
            mock.Setup(m => m.GetSecretValueAsync("kv-secret-name")).ReturnsAsync("my-password");

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(mock.Object));
            var result = await configurationMerger.MergeAsync(Enumerable.Empty<SubscriberConfiguration>(), cosmosSubscribers);

            var expectedConfiguration = new SubscriberConfiguration
            {
                Name = "testevent-captain-hook",
                EventType = "testevent",
                SubscriberName = "captain-hook",
                Uri = "https://cosmos.eshopworld.com/testevent/",
                HttpVerb = "POST",
                AuthenticationConfig = new OidcAuthenticationConfig
                {
                    ClientId = "captain-hook-id",
                    ClientSecret = "my-password",
                    Uri = "https://blah-blah.sts.eshopworld.com",
                    Scopes = new[] { "scope1" }
                }
            };

            result.Should().BeEquivalentTo(new[] { expectedConfiguration }, options => options.RespectingRuntimeTypes());
        }

        [Fact(Skip = "Callback handling not needed as for now"), IsUnit]
        public async Task CosmosSubscriberWithCallback_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "selector")
                    .WithCallback("https://cosmos.eshopworld.com/callback/", "PUT", "selector")
                    .Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(Mock.Of<ISecretProvider>()));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Should().HaveCount(1);
                result.Should().Contain(x => x.Callback.Name == "captain-hook"
                                             && x.Callback.EventType == "testevent"
                                             && x.Callback.Uri == "https://cosmos.eshopworld.com/callback/" && x.Callback.HttpVerb == "PUT");
            }
        }

        [Fact(Skip = "DQL handling not needed as for now"), IsUnit]
        public async Task CosmosSubscriberWithDlq_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent-dlq").WithName("DLQ")
                    .WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "selector")
                    .WithDlq("https://cosmos.eshopworld.com/dlq/", "PUT", "selector")
                    .Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(Mock.Of<ISecretProvider>()));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

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