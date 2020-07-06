using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Director
{
    public class SubscriberConfigurationComparerTests
    {
        private readonly Dictionary<string, SubscriberConfiguration> oldConfigs = new Dictionary<string, SubscriberConfiguration>
        {
            ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
            ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
            ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                 .AddWebhookRequestRule(rb => rb
                    .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                    .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                .Create(),
            ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
        };

        [Fact, IsUnit]
        public void NoChangesDone_NoChangesDetected()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                     .AddWebhookRequestRule(rb => rb
                        .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                        .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                    .Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeFalse();
            result.Added.Should().BeEmpty();
            result.Removed.Should().BeEmpty();
            result.Changed.Should().BeEmpty();
        }

        [Fact, IsUnit]
        public void NewSubscriber_ShouldBeInAddedList()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                     .AddWebhookRequestRule(rb => rb
                        .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                        .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                    .Create(),
                ["event2-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("subscriber1").Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().HaveCount(1).And.Contain(x => x.Key == "event2-subscriber1");
            result.Removed.Should().BeEmpty();
            result.Changed.Should().BeEmpty();
        }

        [Fact, IsUnit]
        public void RemovedSubscriber_ShouldBeInRemovedList()
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().BeEmpty();
            result.Removed.Should().HaveCount(1).And.Contain(x => x.Key == "event2-captain-hook");
            result.Changed.Should().BeEmpty();
        }

        [Theory, IsUnit]
        [MemberData(nameof(ChangedSubscribers))]
        public void ExistingSubscriberChange_ShouldBeInChangedList(SubscriberConfiguration changedSubscriber)
        {
            var newConfigs = new Dictionary<string, SubscriberConfiguration>
            {
                ["event1-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("captain-hook").Create(),
                ["event1-subscriber1"] = new SubscriberConfigurationBuilder().WithType("event1").WithSubscriberName("subscriber1").Create(),
                ["event2-captain-hook"] = changedSubscriber,
                ["event3-captain-hook"] = new SubscriberConfigurationBuilder().WithType("event3").WithSubscriberName("captain-hook").Create(),
            };

            var result = new SubscriberConfigurationComparer().Compare(oldConfigs, newConfigs);

            result.HasChanged.Should().BeTrue();
            result.Added.Should().BeEmpty();
            result.Removed.Should().BeEmpty();
            result.Changed.Should().HaveCount(1).And.Contain(x => x.Key == "event2-captain-hook");
        }

        public static IEnumerable<object[]> ChangedSubscribers
        {
            get
            {
                yield return new object[] {
                     new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                         .AddWebhookRequestRule(rb => rb
                             .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                             .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                         .WithOidcAuthentication()
                         .Create()
                 };
                yield return new object[] {
                     new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                         .AddWebhookRequestRule(rb => rb
                             .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                             .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                         .WithCallback("https://calback.eshopworld.com")
                         .Create()
                 };
                yield return new object[] {
                    new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                        .AddWebhookRequestRule(rb => rb
                            .WithSource("OrderDto", DataType.HttpContent).WithDestination("", DataType.Model)
                            .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com"))
                        .Create()
                };
                yield return new object[] {
                    new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                        .AddWebhookRequestRule(rb => rb
                            .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                            .AddRoute("selector2", "https://blah.blah.selector2.eshopworld.com"))
                        .Create()
                };
                yield return new object[] {
                    new SubscriberConfigurationBuilder().WithType("event2").WithSubscriberName("captain-hook")
                        .AddWebhookRequestRule(rb => rb
                            .WithSource("OrderDto", DataType.Model).WithDestination("", DataType.Model)
                            .AddRoute("selector1", "https://blah.blah.selector1.eshopworld.com")
                            .AddRoute("selector2", "https://blah.blah.selector2.eshopworld.com"))
                        .WithOidcAuthentication()
                        .Create()
                };
            }
        }
    }
}
