using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators.Dtos;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class UpsertWebhookRequestValidator : AbstractValidator<UpsertWebhookRequest>
    {
        public UpsertWebhookRequestValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
            RuleFor(x => x.Endpoint).NotNull()
                .SetValidator(new EndpointDtoValidator());
        }
    }
}