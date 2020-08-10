using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    public class WebhooksEntityValidator: AbstractValidator<WebhooksEntity>
    {
        public WebhooksEntityValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Endpoints)
                .NotEmpty().WithMessage("Webhooks list must contain at list one endpoint");

            RuleFor(x => x.Endpoints)
                .Must(ContainAtMostOneEndpointWithNoSelector)
                .WithMessage("There can be only one endpoint with no selector");

            RuleFor(x => x.Endpoints)
                .Must(NotContainMultipleEndpointsWithTheSameSelector)
                .WithMessage("There cannot be multiple endpoints with the same selector");
        }

        private bool ContainAtMostOneEndpointWithNoSelector(IEnumerable<EndpointEntity> endpoints)
        {
            return endpoints.Count(x => x.Selector == null) <= 1;
        }

        private bool NotContainMultipleEndpointsWithTheSameSelector(IEnumerable<EndpointEntity> endpoints)
        {
            return !endpoints
                .Where(x => x.Selector != null)
                .GroupBy(x => x.Selector)
                .Any(x => x.Count() > 1);
        }
    }
}