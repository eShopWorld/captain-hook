using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using FluentValidation;

namespace CaptainHook.EventHandlerActor.Validation
{
    public class WebhookRequestRuleForRouteAndReplaceValidator : AbstractValidator<WebhookRequestRule>
    {
        public WebhookRequestRuleForRouteAndReplaceValidator()
        {
            RuleFor(x => x.Source).NotNull().SetValidator(new SourceParserLocationValidator());
            RuleFor(x => x.Destination).NotNull().SetValidator(new DestinationLocationValidator());
            RuleFor(x => x.Routes).Must(ContainValidSelectors);
            RuleForEach(x => x.Routes).SetValidator((x, y) => new RouteValidator(x));
        }

        private bool ContainValidSelectors(List<WebhookConfigRoute> routes)
        {
            var selectors = routes?.Select(r => r.Selector).ToList();
            return ContainExactlyOneDefaultSelector(selectors) && AllSelectorsMustBeUnique(selectors);
        }

        private bool ContainExactlyOneDefaultSelector(IList<string> selectors)
        {
            return selectors.Count(x => x == RouteAndReplaceRequestBuilder.DefaultFallbackSelectorKey) == 1;
        }

        private bool AllSelectorsMustBeUnique(IList<string> selectors)
        {
            return selectors.Distinct().Count() == selectors.Count;
        }

        private class SourceParserLocationValidator : AbstractValidator<SourceParserLocation>
        {
            public SourceParserLocationValidator()
            {
                RuleFor(x => x.Path).Null();
                RuleFor(x => x.Replace).Must(kvp => kvp?.ContainsKey(RouteAndReplaceRequestBuilder.SelectorKeyName) == true);
                RuleFor(x => x.Replace).Must(kvp => kvp?.Values.All(v => !string.IsNullOrWhiteSpace(v)) == true);
            }
        }

        private class DestinationLocationValidator : AbstractValidator<ParserLocation>
        {
            public DestinationLocationValidator()
            {
                RuleFor(x => x.RuleAction).Equal(RuleAction.RouteAndReplace);
            }
        }

        private class RouteValidator : AbstractValidator<WebhookConfigRoute>
        {
            private static readonly Regex _extractSelectorsFromUri = new Regex("(?<=\\{).+?(?=\\})", RegexOptions.Compiled);
            private readonly WebhookRequestRule _rule;

            public RouteValidator(WebhookRequestRule rule)
            {
                _rule = rule;
                RuleFor(x => x.Selector).NotEmpty();
                RuleFor(x => x.Uri).NotEmpty()
                    .Must(ContainOnlyDefinedSelectors);
            }

            private bool ContainOnlyDefinedSelectors(string uri)
            {
                if (uri == null)
                    return true;

                var values = _extractSelectorsFromUri.Matches(uri).Select(m => m.Value);
                return values.All(value => _rule.Source.Replace.ContainsKey(value));
            }
        }
    }
}