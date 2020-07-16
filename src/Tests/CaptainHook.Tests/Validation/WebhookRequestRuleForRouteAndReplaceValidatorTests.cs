using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Validation
{
    public class WebhookRequestRuleForRouteAndReplaceValidatorTests
    {
        private static WebhookRequestRuleBuilder GetValidWebhookRequestRuleBuilder()
        {
            var ruleBuilder = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"));

            return ruleBuilder;
        }

        [Fact, IsUnit]
        public void When_Rule_is_valid_then_validation_should_not_fail()
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();

            var result = new WebhookRequestRuleForRouteAndReplaceValidator().Validate(rule);

            result.IsValid.Should().BeTrue();
        }

        [Theory, IsUnit]
        [InlineData(RuleAction.Route)]
        [InlineData(RuleAction.Replace)]
        [InlineData(RuleAction.Add)]
        public void When_Destination_Location_is_not_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Destination.RuleAction = ruleAction;

            VerifySingleFailure(rule);
        }

        [Theory, IsUnit]
        [InlineData(Location.Uri)]
        [InlineData(Location.Header)]
        [InlineData(Location.HttpContent)]
        [InlineData(Location.HttpStatusCode)]
        public void When_Source_Location_is_not_Body_then_validation_should_fail(Location location)
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Source.Location = location;

            VerifySingleFailure(rule);
        }

        [Fact, IsUnit]
        public void When_Replace_does_not_contain_an_item_with_key_selector_then_validation_should_fail()
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("not-selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{not-selector}.company.com/order/{orderCode}"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        public void When_Replace_contains_default_selector_with_empty_value_then_validation_should_fail(string invalidValue)
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", invalidValue))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.company.com/"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        public void When_Replace_contains_nondefault_selector_with_empty_value_then_validation_should_fail(string invalidValue)
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "$.Code")
                    .AddReplace("custom-selector", invalidValue))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.company.com/custom/{custom-selector}"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Fact, IsUnit]
        public void When_Replace_does_not_contain_key_which_exist_in_Route_Uri_then_validation_should_fail()
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://test-api-{selector}.company.com/api/v2/{missing-action}/Put/{orderCode}"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        public void When_Route_contains_empty_Uri_then_validation_should_fail(string invalidValue)
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "something"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri(invalidValue))
                .Create();

            VerifySingleFailure(rule);
        }

        [Fact, IsUnit]
        public void When_more_than_one_Route_Selector_is_default_then_validation_should_fail()
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.other-company.com/order/{orderCode}"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        public void When_Route_Uri_selector_is_empty_then_validation_should_fail(string invalidValue)
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector(invalidValue)
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                .Create();

            VerifySingleFailure(rule);
        }

        [Fact, IsUnit]
        public void When_Route_Uri_selector_is_not_unique_then_validation_should_fail()
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("*")
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("non-unique")
                    .WithUri("https://api-{selector}.another-company.com/order/{orderCode}"))
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("non-unique")
                    .WithUri("https://api-{selector}.yet-another-company.com/order/{orderCode}"))
                .Create();

            VerifySingleFailure(rule);
        }

        private void VerifySingleFailure(WebhookRequestRule rule)
        {
            var result = new WebhookRequestRuleForRouteAndReplaceValidator().Validate(rule);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
        }
    }
}