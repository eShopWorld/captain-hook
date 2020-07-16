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
            RuleFor(x => x.WebhookRequestRules)
                .NotEmpty().Must(ContainAtLeastOneRuleWithRouteAndReplace);

            RuleForEach(x => x.WebhookRequestRules)
                .Where(x => x.Destination?.RuleAction == RuleAction.RouteAndReplace)
                .SetValidator(new WebhookRequestRuleForRouteAndReplaceValidator());
        }

        private static bool ContainAtLeastOneRuleWithRouteAndReplace(List<WebhookRequestRule> rules)
        {
            return rules.Any(rule => rule.Destination?.RuleAction == RuleAction.RouteAndReplace);
        }
    }
}
