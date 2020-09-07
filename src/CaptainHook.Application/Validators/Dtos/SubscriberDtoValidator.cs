using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Webhooks).NotEmpty()
                .SetValidator(new WebhooksDtoValidator());
        }
    }
}
