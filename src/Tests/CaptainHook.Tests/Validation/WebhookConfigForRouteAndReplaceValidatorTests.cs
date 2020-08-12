using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Validation;
using CaptainHook.Tests.Builders;
using CaptainHook.TestsInfrastructure;
using CaptainHook.TestsInfrastructure.TestsData;
using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
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
        public void When_WebhookConfig_contains_one_proper_RouteAndReplace_rule_then_no_failures_should_be_returned()
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();

            var result = _validator.TestValidate(webhookConfig);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [GetInvalidEnumValues(RuleAction.RouteAndReplace)]
        public void When_no_Rule_has_Destination_RuleAction_set_to_RouteAndReplace_then_validation_should_fail(RuleAction ruleAction)
        {
            var webhookConfig = GetValidWebhookConfigBuilder().Create();
            webhookConfig.WebhookRequestRules[0].Destination.RuleAction = ruleAction;

            var result = _validator.TestValidate(webhookConfig);

            result.ShouldHaveValidationErrorFor(x => x.WebhookRequestRules);
        }

        [Fact, IsUnit]
        public void When_WebhookConfig_contains_another_rule_with_Destination_RuleAction_Route_then_validation_should_fail()
        {
            var webhookConfig = GetValidWebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithDestination(ruleAction: RuleAction.Route))
                .Create();

            var result = _validator.TestValidate(webhookConfig);

            result.ShouldHaveValidationErrorFor(x => x.WebhookRequestRules);
        }

        [Fact, IsUnit]
        public void When_WebhookConfig_contains_another_rule_with_Destination_RuleAction_Add_and_Location_Uri_then_validation_should_fail()
        {
            var webhookConfig = GetValidWebhookConfigBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder
                    .WithDestination(ruleAction: RuleAction.Add, location: Location.Uri))
                .Create();

            var result = _validator.TestValidate(webhookConfig);

            result.ShouldHaveValidationErrorFor(x => x.WebhookRequestRules);
        }
    }
}
