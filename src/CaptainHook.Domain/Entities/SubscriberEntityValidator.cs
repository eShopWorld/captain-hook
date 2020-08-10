using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    public class SubscriberEntityValidator : AbstractValidator<SubscriberEntity>
    {
        public SubscriberEntityValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Webhooks)
                .NotEmpty().WithMessage("Webhooks are required for Subscriber definition")
                .SetValidator(new WebhooksEntityValidator());
        }
    }
}