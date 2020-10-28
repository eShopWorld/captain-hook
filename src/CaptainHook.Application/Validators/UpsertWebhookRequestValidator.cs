using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators.Dtos;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class UpsertWebhookRequestValidator : AbstractValidator<UpsertWebhookRequest>
    {
        public UpsertWebhookRequestValidator()
        {
            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .MaximumLength(50);
            RuleFor(x => x.Selector).NotEmpty();
            RuleFor(x => x.Endpoint).Cascade(CascadeMode.Stop)
                .NotNull()
                .SetValidator(request => new UpsertEndpointDtoValidator(request.Selector));
        }
    }
}