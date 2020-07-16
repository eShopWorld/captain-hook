using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Validation
{
    public class WebhookConfigForRouteAndReplaceValidatorTests
    {
        private readonly WebhookConfigForRouteAndReplaceValidator _validator = new WebhookConfigForRouteAndReplaceValidator();

        private static WebhookConfigBuilder GetValidWebhookConfigBuilder()
        {
            var webhookConfigBuilder = new WebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithSource(sourceBuilder => sourceBuilder
                        .WithLocation(Location.Body)
                        .AddReplace("selector", "something")
                        .AddReplace("orderCode", "other-value"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(routeBuilder => routeBuilder
                        .WithSelector("*")
                        .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                );

            return webhookConfigBuilder;
        }

        [Fact, IsUnit]
        public void When_WebhookConfig_contains_one_proper_ReouteAndReplace_rule_then_no_failures_should_be_returned()
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void When_WebhookConfig_contains_more_rules_then_no_failures_should_be_returned()
        {
            var webhookConfigBuilder = new WebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithSource(sourceBuilder => sourceBuilder
                        .WithLocation(Location.Body)
                        .AddReplace("selector", "something")
                        .AddReplace("orderCode", "other-value"))
                    .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                    .AddRoute(routeBuilder => routeBuilder
                        .WithSelector("*")
                        .WithUri("https://api-{selector}.company.com/order/{orderCode}")))
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder.WithSource(sourceBuilder => sourceBuilder
                    .WithRuleAction(RuleAction.Replace))
                );

            var webhookConfig = webhookConfigBuilder.Create();

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData(RuleAction.Add)]
        [InlineData(RuleAction.Replace)]
        [InlineData(RuleAction.Route)]
        public void When_no_Rule_has_Destination_RuleAction_set_to_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Destination.RuleAction = ruleAction;

            var result = _validator.Validate(webhookConfig);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }
    }
}
