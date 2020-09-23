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
                .SetValidator(new WebhooksDtoValidator(WebhooksDtoType.Webhook));

            RuleFor(x => x.Callbacks)
                .SetValidator(new WebhooksDtoValidator(WebhooksDtoType.Callback));

            RuleFor(x => x.DlqHooks)
                .SetValidator(new WebhooksDtoValidator(WebhooksDtoType.DlqHook));
        }
    }
}
