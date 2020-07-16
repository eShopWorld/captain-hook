using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using CaptainHook.Tests.TestsInfrastructure;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Xunit;

namespace CaptainHook.Tests.Validation
{
    public class WebhookRequestRuleForRouteAndReplaceValidatorTests
    {
        private static WebhookRequestRuleBuilder GetValidWebhookRequestRuleBuilder()
        {
            var ruleBuilder = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body).WithPath(null)
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
        [InvalidEnumValues(RuleAction.RouteAndReplace)]
        public void When_Destination_Location_is_not_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Destination.RuleAction = ruleAction;

            VerifySingleFailure(rule, nameof(ParserLocation.RuleAction));
        }

        [Theory, IsUnit]
        [InvalidEnumValues(Location.Body)]
        public void When_Source_Location_is_not_Body_then_validation_should_fail(Location location)
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Source.Location = location;

            VerifySingleFailure(rule, nameof(SourceParserLocation.Location));
        }

        [Fact, IsUnit]
        public void When_Source_Path_is_not_null_then_validation_should_fail()
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Source.Path = "some-path";

            VerifySingleFailure(rule, nameof(SourceParserLocation.Path));
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

            VerifySingleFailure(rule, nameof(SourceParserLocation.Replace));
        }

        [Theory, IsUnit]
        [MemberData(nameof(EmptyStrings))]
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

            VerifySingleFailure(rule, nameof(SourceParserLocation.Replace));
        }

        [Theory, IsUnit]
        [MemberData(nameof(EmptyStrings))]
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

            VerifySingleFailure(rule, nameof(SourceParserLocation.Replace));
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

            VerifySingleFailure(rule, nameof(WebhookConfigRoute.Uri));
        }

        [Theory, IsUnit]
        [MemberData(nameof(EmptyStrings))]
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

            VerifySingleFailure(rule, nameof(WebhookConfigRoute.Uri));
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

            VerifySingleFailure(rule, nameof(WebhookRequestRule.Routes));
        }

        [Theory, IsUnit]
        [MemberData(nameof(EmptyStrings))]
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

            VerifySingleFailure(rule, nameof(WebhookConfigRoute.Selector));
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

            VerifySingleFailure(rule, nameof(WebhookRequestRule.Routes));
        }

        public static IEnumerable<object[]> EmptyStrings =>
           new List<object[]>
           {
                new object[] { null },
                new object[] { string.Empty },
                new object[] { "   " },
           };

        private void VerifySingleFailure(WebhookRequestRule rule, string propertyName)
        {
            var result = new WebhookRequestRuleForRouteAndReplaceValidator().Validate(rule);

            using var assertionScope = new AssertionScope();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().EndWith(propertyName);
        }
    }
}