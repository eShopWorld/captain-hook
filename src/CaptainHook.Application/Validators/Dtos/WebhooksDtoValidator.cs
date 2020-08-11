using CaptainHook.Contract;
using FluentValidation;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.Application.Validators.Dtos
{
    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
    {
        private static JObject _jObject = new JObject();

        public WebhooksDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.SelectionRule).NotEmpty()
                .Must(BeValidJsonPathExpression).WithMessage("The SelectionRule must be a valid JSONPath expression");

            RuleFor(x => x.Endpoints)
                .NotNull().WithMessage("Webhooks list must contain at list one endpoint")
                .NotEmpty().WithMessage("Webhooks list must contain at list one endpoint");

            RuleFor(x => x.Endpoints)
                .Must(ContainAtMostOneEndpointWithNoSelector)
                .WithMessage("There can be only one endpoint with no selector");

            RuleFor(x => x.Endpoints)
                .Must(NotContainMultipleEndpointsWithTheSameSelector)
                .WithMessage("There cannot be multiple endpoints with the same selector");

            RuleForEach(x => x.Endpoints)
                .SetValidator(new EndpointDtoValidator());
        }

        private bool ContainAtMostOneEndpointWithNoSelector(List<EndpointDto> endpoints)
        {
            return endpoints?.Count(x => x.Selector == null) <= 1;
        }

        private bool NotContainMultipleEndpointsWithTheSameSelector(List<EndpointDto> endpoints)
        {
            return !(endpoints ?? Enumerable.Empty<EndpointDto>())
                .Where(x => x.Selector != null)
                .GroupBy(x => x.Selector)
                .Any(x => x.Count() > 1);
        }

        private bool BeValidJsonPathExpression(string selectionRule)
        {
            if(!selectionRule.StartsWith('$'))
            {
                return false;
            }

            try
            {
                _jObject.SelectToken(selectionRule, false);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}