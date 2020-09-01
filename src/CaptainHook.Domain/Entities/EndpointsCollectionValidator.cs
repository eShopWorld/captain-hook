using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    public class EndpointsCollectionValidator : AbstractValidator<IEnumerable<EndpointEntity>>
    {
        public EndpointsCollectionValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x)
                .NotEmpty().WithMessage("Webhooks list must contain at list one endpoint")
                .Must(NotContainMultipleEndpointsWithTheSameSelector)
                .WithMessage("There cannot be multiple endpoints with the same selector");
        }

        private static bool NotContainMultipleEndpointsWithTheSameSelector(IEnumerable<EndpointEntity> endpoints)
        {
            return !endpoints
                .Where(x => x.Selector != null)
                .GroupBy(x => x.Selector)
                .Any(x => x.Count() > 1);
        }
    }
}