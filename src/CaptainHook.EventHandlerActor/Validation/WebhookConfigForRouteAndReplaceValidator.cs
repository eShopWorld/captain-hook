using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using FluentValidation;

namespace CaptainHook.EventHandlerActor.Validation
{
    public class WebhookConfigForRouteAndReplaceValidator : AbstractValidator<WebhookConfig>
    {
        public WebhookConfigForRouteAndReplaceValidator()
        {
            RuleFor(x => x.WebhookRequestRules).NotEmpty().Must(BeValid);

            RuleForEach(x => x.WebhookRequestRules)
                .Where(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace)
                .SetValidator(new WebhookRequestRuleForRouteAndReplaceValidator());
        }

        private bool BeValid(List<WebhookRequestRule> rules)
        {
            return ContainAtLeastOneRuleWithRuleActionRouteAndReplace(rules)
                   && NotContainRulesWithRuleActionRoute(rules)
                   && NotContainRulesWithRuleActionAddAndLocationUri(rules);
        }

        private static bool ContainAtLeastOneRuleWithRuleActionRouteAndReplace(List<WebhookRequestRule> rules)
        {
            return rules.Any(rule => rule.Destination?.RuleAction == RuleAction.RouteAndReplace);
        }

        private static bool NotContainRulesWithRuleActionRoute(List<WebhookRequestRule> rules)
        {
            return rules.All(rule => rule.Destination?.RuleAction != RuleAction.Route);
        }

        private static bool NotContainRulesWithRuleActionAddAndLocationUri(List<WebhookRequestRule> rules)
        {
            return rules.All(rule => !(rule.Destination?.RuleAction == RuleAction.Add && rule.Destination?.Location == Location.Uri));
        }
    }
}
