using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Tests.Validation
{
    public class WebhookRequestRuleForRouteAndReplaceValidatorTests
    {
        private readonly WebhookRequestRuleForRouteAndReplaceValidator _validator = new WebhookRequestRuleForRouteAndReplaceValidator();

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

            var result = _validator.TestValidate(rule);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [GetInvalidEnumValues(RuleAction.RouteAndReplace)]
        public void When_Destination_Location_is_not_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Destination.RuleAction = ruleAction;

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Destination.RuleAction);
        }

        [Fact, IsUnit]
        public void When_Source_Path_is_not_null_then_validation_should_fail()
        {
            var rule = GetValidWebhookRequestRuleBuilder().Create();
            rule.Source.Path = "some-path";

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Source.Path);
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Source.Replace);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Source.Replace);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Source.Replace);
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor("Routes[0].Uri");
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor("Routes[0].Uri");
        }

        [Fact, IsUnit]
        public void When_there_is_no_default_Route_Selector_then_validation_should_fail()
        {
            var rule = new WebhookRequestRuleBuilder()
                .WithSource(sourceBuilder => sourceBuilder
                    .WithLocation(Location.Body).WithRuleAction(RuleAction.RouteAndReplace)
                    .AddReplace("selector", "something")
                    .AddReplace("orderCode", "other-value"))
                .WithDestination(ruleAction: RuleAction.RouteAndReplace)
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("brand1")
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                .AddRoute(routeBuilder => routeBuilder
                    .WithSelector("brand2")
                    .WithUri("https://api-{selector}.other-company.com/order/{orderCode}"))
                .Create();

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Routes);
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Routes);
        }

        [Theory, IsUnit]
        [ClassData(typeof(EmptyStrings))]
        public void When_Route_Uri_selector_is_empty_then_validation_should_fail(string invalidValue)
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
                    .WithSelector(invalidValue)
                    .WithUri("https://api-{selector}.company.com/order/{orderCode}"))
                .Create();

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor("Routes[1].Selector");
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

            var result = _validator.TestValidate(rule);

            result.ShouldHaveValidationErrorFor(x => x.Routes);
        }
    }
}