using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            RuleFor(x => x.Webhooks).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new WebhooksDtoValidator("Webhooks"));

            RuleFor(x => x.Callbacks)
                .SetValidator(new WebhooksDtoValidator("Callbacks"));

            RuleFor(x => x.Dlq)
                .SetValidator(new WebhooksDtoValidator("DLQ"));
        }
    }
}
