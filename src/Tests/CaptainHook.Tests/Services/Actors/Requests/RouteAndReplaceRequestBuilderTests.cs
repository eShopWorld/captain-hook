using System;
using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using CaptainHook.Tests.Builders;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Services.Actors.Requests
{
    public class RouteAndReplaceRequestBuilderTests
    {
        [IsUnit]
        [Theory]
        [MemberData(nameof(UriDataRouteAndReplace))]
        public void UriConstructionRouteAndReplaceTests(WebhookConfig config, string payload, string expectedUri)
        {
            var uri = new RouteAndReplaceRequestBuilder(Mock.Of<IBigBrother>()).BuildUri(config, payload);

            uri.Should().BeEquivalentTo(new Uri(expectedUri));
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(WebhookConfigData))]
        public void SelectWebhookConfigTests(WebhookConfig config, string payload, WebhookConfig expectedWebhookConfig)
        {
            var actualConfig = new RouteAndReplaceRequestBuilder(Mock.Of<IBigBrother>()).SelectWebhookConfig(config, payload);

            actualConfig.Should().BeEquivalentTo(expectedWebhookConfig);
        }

        public static IEnumerable<object[]> UriDataRouteAndReplace =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                                .WithSource(source => source
                                    .AddReplace("selector", "$.TenantCode")
                                    .AddReplace("orderCode", "$.OrderCode"))
                                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                                .AddRoute(route => route
                                    .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                    .WithSelector("Brand1")
                                    .WithNoAuthentication())
                                .AddRoute(route => route
                                    .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                    .WithSelector("*")
                                    .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/9744b831-df2c-4d59-9d9d-691f4121f73a"
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"DEV13:00026804\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    "https://blah.blah.tenant1.eshopworld.com/webhook/DEV13%3A00026804"
                }
            };

        public static IEnumerable<object[]> WebhookConfigData =>
            new List<object[]>
            {
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("Brand1")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithSelector("*")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("tenant0")
                                .WithNoAuthentication())
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant0\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.Brand1.eshopworld.com/webhook/{orderCode}")
                        .WithSelector("tenant0")
                        .WithNoAuthentication()
                        .Create()
                },
                new object[]
                {
                    new WebhookConfigBuilder()
                        .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                            .WithSource(source => source
                                .AddReplace("selector", "$.TenantCode")
                                .AddReplace("orderCode", "$.OrderCode"))
                            .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                            .AddRoute(route => route
                                .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                                .WithSelector("*")
                                .WithNoAuthentication()))
                        .Create(),
                    "{\"OrderCode\":\"9744b831-df2c-4d59-9d9d-691f4121f73a\", \"BrandType\":\"Brand2\", \"TenantCode\":\"tenant1\"}",
                    new WebhookConfigRouteBuilder()
                        .WithUri("https://blah.blah.{selector}.eshopworld.com/webhook/{orderCode}")
                        .WithSelector("*")
                        .WithNoAuthentication()
                        .Create()
                }
            };
    }
}