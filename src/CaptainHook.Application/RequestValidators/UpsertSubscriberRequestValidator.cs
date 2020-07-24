using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using FluentValidation;
using FluentValidation.Validators;

namespace CaptainHook.Application.RequestValidators
{
    public class UpsertWebhookRequestValidator : AbstractValidator<UpsertWebhookRequest>
    {
        public UpsertWebhookRequestValidator()
        {
            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
        }
    }

    public class AuthenticationDtoValidator : AbstractValidator<AuthenticationDto>
    {
        public AuthenticationDtoValidator()
        {
        }
    }
}