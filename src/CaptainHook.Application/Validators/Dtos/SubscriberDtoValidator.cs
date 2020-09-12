using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Webhooks).NotEmpty()
                .SetValidator(new WebhooksDtoValidator("Webhooks"));

            RuleFor(x => x.Callbacks)
                .SetValidator(new WebhooksDtoValidator("Callbacks"));
        }
    }
}
