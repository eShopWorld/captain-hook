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
        }
    }

    public class AuthenticationDtoValidator : AbstractValidator<AuthenticationDto>
    {
        public AuthenticationDtoValidator()
        {
        }
    }
}