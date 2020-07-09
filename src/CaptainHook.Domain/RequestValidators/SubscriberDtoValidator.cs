using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Api.Controllers
{
    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("foobar").WithMessage("Provide a Name, please!");
            RuleFor(x => x.EventName).Length(2, 10);
            RuleFor(x => x.Webhooks).NotNull().SetValidator(new WebhooksDtoValidator());
        }
    }
}