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
        private static readonly BasicAuthenticationEntity BasicAuthenticationEntity = new BasicAuthenticationEntity("username", "secret-name");
        private static readonly OidcAuthenticationEntity OidcAuthenticationEntity = new OidcAuthenticationEntity("captain-hook-id", "kv-secret-name", "https://blah-blah.sts.eshopworld.com", new[] { "scope1" });

        private static readonly BasicAuthenticationConfig BasicAuthenticationConfig = new BasicAuthenticationConfig
        {
            Username = "username",
            Password = "password"
        };

        private static readonly OidcAuthenticationConfig OidcAuthenticationConfig = new OidcAuthenticationConfig
        {
            ClientId = "captain-hook-id",
            ClientSecret = "secret",
            Uri = "https://blah-blah.sts.eshopworld.com",
            Scopes = new[] { "scope1" }
        };

        private readonly Mock<ISecretProvider> _secretProvider;
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "POST", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "GET", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "PUT", OidcAuthenticationEntity, OidcAuthenticationConfig },
                new object[] { "POST", BasicAuthenticationEntity, BasicAuthenticationConfig },
                new object[] { "GET", BasicAuthenticationEntity, BasicAuthenticationConfig },
                new object[] { "PUT", BasicAuthenticationEntity, BasicAuthenticationConfig }
            };

        private static readonly WebhookRequestRule StatusCodeRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpStatusCode
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.Property,
                Path = "StatusCode"
            }
        };

        private static readonly WebhookRequestRule ContentRequestRule = new WebhookRequestRule
        {
            Source = new SourceParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.HttpContent
            },
            Destination = new ParserLocation
            {
                Location = Location.Body,
                RuleAction = RuleAction.Add,
                Type = DataType.String,
                Path = "Content"
            }
        };

        public ConfigurationMergerTests()
        {
            _secretProvider = new Mock<ISecretProvider>();
            _secretProvider.Setup(x => x.GetSecretValueAsync("secret-name"))
                .ReturnsAsync("password");
            _secretProvider.Setup(x => x.GetSecretValueAsync("kv-secret-name"))
                .ReturnsAsync("secret");
        }

        [Fact, IsUnit]
        public async Task OnlyKVSubscribers_AllInResult()
        {
            var kvSubscribers = new[]
            {
                new SubscriberConfigurationBuilder().WithName("testevent").WithCallback().Create(),
                new SubscriberConfigurationBuilder().WithName("testevent").WithSubscriberName("subscriber1").Create(),
                new SubscriberConfigurationBuilder().WithName("testevent.completed").Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(kvSubscribers, new List<SubscriberEntity>());

            using (new AssertionScope())
            {
                result.Data.Should().HaveCount(3);
                result.Data.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "captain-hook");
                result.Data.Should().Contain(x => x.Name == "testevent" && x.SubscriberName == "subscriber1");
                result.Data.Should().Contain(x => x.Name == "testevent.completed" && x.SubscriberName == "captain-hook");
            }
        }

        [Fact, IsUnit]
        public async Task OnlyCosmosSubscribers_AllInResult()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "*", BasicAuthenticationEntity).Create(),
                new SubscriberBuilder().WithEvent("testevent.completed").WithWebhook("https://cosmos.eshopworld.com/testevent-completed/", "POST", "*", BasicAuthenticationEntity).Create(),
                new SubscriberBuilder().WithEvent("testevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/testevent2/", "POST", "*", BasicAuthenticationEntity).Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Data.Should().HaveCount(3);
                result.Data.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "captain-hook");
                result.Data.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "subscriber1");
                result.Data.Should().Contain(x => x.EventType == "testevent.completed" && x.SubscriberName == "captain-hook");
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
                new SubscriberBuilder().WithEvent("TESTevent").WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "*", BasicAuthenticationEntity).Create(),
                new SubscriberBuilder().WithEvent("newtestevent.completed").WithWebhook("https://cosmos.eshopworld.com/newtestevent-completed/", "POST", "*", BasicAuthenticationEntity).Create(),
                new SubscriberBuilder().WithEvent("newtestevent").WithName("subscriber1").WithWebhook("https://cosmos.eshopworld.com/newtestevent2/", "POST", "*", BasicAuthenticationEntity).Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(kvSubscribers, cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Data.Should().HaveCount(5);

                result.Data.Should().Contain(x => x.EventType == "TESTevent" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/testevent/");

                result.Data.Should().Contain(x => x.EventType == "testevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://blah.blah.eshopworld.com");
                result.Data.Should().Contain(x => x.EventType == "testevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://blah.blah.eshopworld.com");

                result.Data.Should().Contain(x => x.EventType == "newtestevent" && x.SubscriberName == "subscriber1" && x.Uri == "https://cosmos.eshopworld.com/newtestevent2/");
                result.Data.Should().Contain(x => x.EventType == "newtestevent.completed" && x.SubscriberName == "captain-hook" && x.Uri == "https://cosmos.eshopworld.com/newtestevent-completed/");
            }
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task CosmosSubscriberWithAuthentication_ShouldBeMappedProperly(string httpVerb, AuthenticationEntity authenticationEntity, AuthenticationConfig authenticationConfig)
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder()
                    .WithEvent("testevent")
                    .WithWebhook(
                        "https://cosmos.eshopworld.com/testevent/",
                        httpVerb,
                        "*",
                        authenticationEntity
                    ).Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(Enumerable.Empty<SubscriberConfiguration>(), cosmosSubscribers);

            var expectedConfiguration = new SubscriberConfigurationBuilder()
                .WithName("testevent-captain-hook")
                .WithType("testevent")
                .WithSubscriberName("captain-hook")
                .WithUri("https://cosmos.eshopworld.com/testevent/")
                .WithHttpVerb(httpVerb)
                .WithAuthentication(authenticationConfig)
                .Create();

            result.Data.Should().BeEquivalentTo(new[] { expectedConfiguration }, options => options.RespectingRuntimeTypes());
        }

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public async Task CosmosSubscriberWithCallback_ShouldBeMappedProperly(string httpVerb, AuthenticationEntity authenticationEntity, AuthenticationConfig authenticationConfig)
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder()
                    .WithEvent("testevent")
                    .WithWebhook("https://cosmos.eshopworld.com/testevent/", httpVerb, "*", authenticationEntity)
                    .WithCallback("https://cosmos.eshopworld.com/callback/", httpVerb, "*", authenticationEntity)
                    .Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Data.Should().HaveCount(1);

                var callback = result.Data.First().Callback;
                callback.Should().NotBeNull();
                callback.Uri.Should().Be("https://cosmos.eshopworld.com/callback/");
                callback.AuthenticationConfig.Should().BeEquivalentTo(authenticationConfig);
                callback.HttpVerb.Should().Be(httpVerb);
                callback.WebhookRequestRules.Should()
                    .ContainEquivalentOf(ContentRequestRule).And
                    .ContainEquivalentOf(StatusCodeRequestRule).And
                    .HaveCount(2);
            }
        }

        [Fact(Skip = "DLQ handling not needed as for now"), IsUnit]
        public async Task CosmosSubscriberWithDlq_ShouldBeMappedProperly()
        {
            var cosmosSubscribers = new[]
            {
                new SubscriberBuilder().WithEvent("testevent-dlq").WithName("DLQ")
                    .WithWebhook("https://cosmos.eshopworld.com/testevent/", "POST", "selector")
                    .WithDlqhook("https://cosmos.eshopworld.com/dlq/", "PUT", "selector")
                    .Create(),
            };

            var configurationMerger = new ConfigurationMerger(new SubscriberEntityToConfigurationMapper(_secretProvider.Object));
            var result = await configurationMerger.MergeAsync(new List<SubscriberConfiguration>(), cosmosSubscribers);

            using (new AssertionScope())
            {
                result.Data.Should().HaveCount(2);
                result.Data.Should().Contain(x => x.Name == "testevent-dlq"
                                             && x.SubscriberName == "DLQ"
                                             && x.WebhookRequestRules.SelectMany(w => w.Routes).All(r => r.Uri == "https://cosmos.eshopworld.com/dlq/"));
            }
        }
    }
}