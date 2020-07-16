using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using FluentValidation;

namespace CaptainHook.EventHandlerActor.Validation
{
    public class WebhookRequestRuleForRouteAndReplaceValidator : AbstractValidator<WebhookRequestRule>
    {
        public WebhookRequestRuleForRouteAndReplaceValidator()
        {
            RuleFor(x => x.Source).NotNull().SetValidator(new SourceParserLocationValidator());
            RuleFor(x => x.Destination).NotNull().SetValidator(new DestinationLocationValidator());
            RuleFor(x => x.Routes).Must(HaveUniqueSelectors);
            RuleForEach(x => x.Routes).SetValidator(new RouteValidator());
        }

        private bool HaveUniqueSelectors(List<WebhookConfigRoute> routes)
        {
            return routes?.Select(x => x.Selector).Distinct().Count() == routes?.Count;
        }

        private class RouteValidator : AbstractValidator<WebhookConfigRoute>
        {
            public RouteValidator()
            {
                RuleFor(x => x.Selector).NotEmpty();
                RuleFor(x => x.Uri).NotEmpty();
            }
        }

        private class SourceParserLocationValidator : AbstractValidator<SourceParserLocation>
        {
            public SourceParserLocationValidator()
            {
                RuleFor(x => x.Location).Equal(Location.Body);
                RuleFor(x => x.Replace).Must(kvp => kvp?.ContainsKey("selector") == true);
                RuleFor(x => x.Replace).Must(kvp => kvp?.Values.All(v => !string.IsNullOrEmpty(v)) == true);
            }
        }

        private class DestinationLocationValidator : AbstractValidator<ParserLocation>
        {
            public DestinationLocationValidator()
            {
                RuleFor(x => x.RuleAction).Equal(RuleAction.RouteAndReplace);
            }
        }
    }
}