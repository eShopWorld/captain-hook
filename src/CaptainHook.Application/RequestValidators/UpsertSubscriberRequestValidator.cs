using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.RequestValidators
{
    public class UpsertWebhookRequestValidator : AbstractValidator<UpsertWebhookRequest>
    {
        public UpsertWebhookRequestValidator()
        {
            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
            RuleFor(x => x.Endpoint).NotNull()
                .SetValidator(new EndpointDtoValidator());
        }
    }

    public class EndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public EndpointDtoValidator()
        {
            RuleFor(x => x.HttpVerb).NotEmpty();
            RuleFor(x => x.Uri).NotEmpty();
            RuleFor(x => x.Authentication).NotNull()
                .SetValidator(new AuthenticationDtoValidator());
        }
    }

    public class AuthenticationDtoValidator : AbstractValidator<AuthenticationDto>
    {
        public AuthenticationDtoValidator()
        {
            RuleFor(x => x.Type).NotEmpty();
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Uri).NotEmpty();
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecret).NotNull();
        }

        private class ClientSecretDtoValidator : AbstractValidator<ClientSecretDto>
        {
            public ClientSecretDtoValidator()
            {
                RuleFor(x => x.Name).NotEmpty();
                RuleFor(x => x.Vault).NotEmpty();
            }
        }
    }
}