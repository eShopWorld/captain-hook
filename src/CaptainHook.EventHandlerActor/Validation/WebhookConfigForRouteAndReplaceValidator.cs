﻿using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using FluentValidation;
using FluentValidation.Validators;

namespace CaptainHook.EventHandlerActor.Validation
{
    public class WebhookConfigForRouteAndReplaceValidator : AbstractValidator<WebhookConfig>
    {
        public WebhookConfigForRouteAndReplaceValidator()
        {
            //RuleFor(x => x.WebhookRequestRules).SetValidator(new WebhookRequestRulesValidator());

            //RuleFor(x => x.WebhookRequestRules).Must((config, list, arg3) => { } )

            RuleFor(x => x.WebhookRequestRules)
                .Must(rules =>
                    rules.Any(rule => rule.Destination?.RuleAction == RuleAction.RouteAndReplace)
                    && rules.All(rule => rule.Source.Replace.ContainsKey("selector"))
                );

            RuleForEach(x => x.WebhookRequestRules).SetValidator(new WebhookRequestRuleValidator());

            //RuleForEach(x => x.WebhookRequestRules).Must(rule => rule.Source.Location == Location.Body);
        }
    }

    public class WebhookRequestRulesValidator : AbstractValidator<List<WebhookRequestRule>>
    {
        public WebhookRequestRulesValidator()
        {
            RuleFor(rules =>
                rules.Any(rule => rule.Destination.RuleAction == RuleAction.RouteAndReplace)
                && rules.Any(rule => rule.Source.Replace.Any(kvp => kvp.Key == "selector" && !string.IsNullOrEmpty(kvp.Value)))
                );

            //RuleForEach(rules => rules).Must(rule => rule.Source.Location == Location.Body);
        }
    }

    public class WebhookRequestRuleValidator : AbstractValidator<WebhookRequestRule>
    {
        public WebhookRequestRuleValidator()
        {
            //RuleFor(x => x.Source.Location).Equal(Location.Body);
            RuleFor(x => x.Source).NotNull().SetValidator(new SourceParserLocationValidator());
        }
    }

    public class SourceParserLocationValidator : AbstractValidator<SourceParserLocation>
    {
        public SourceParserLocationValidator()
        {
            RuleFor(x => x.Location).Equal(Location.Body);
        }
    }
}
