using System;
using System.Linq;
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
            CascadeMode = CascadeMode.StopOnFirstFailure;

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
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.HttpVerb).NotEmpty()
                .SetValidator(new HttpVerbValidator());
            RuleFor(x => x.Uri).NotEmpty()
                .SetValidator(new UriValidator());
            RuleFor(x => x.Authentication).NotNull()
                .SetValidator(new AuthenticationDtoValidator());
        }
    }

    internal class UriValidator : PropertyValidator
    {
        public UriValidator()
            : base("{PropertyName} must be valid URI.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var rawValue = context.PropertyValue as string;
            return Uri.TryCreate(rawValue, UriKind.Absolute, out Uri _);
        }
    }

    internal class HttpVerbValidator : PropertyValidator
    {
        private readonly string[] _validVerbs = { "POST", "PUT", "GET" };

        public HttpVerbValidator()
            : base("{PropertyName} must be valid HTTP verb.")
        {
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            var httpVerb = context.PropertyValue as string;
            return _validVerbs.Contains(httpVerb);
        }
    }

    public class AuthenticationDtoValidator : AbstractValidator<AuthenticationDto>
    {
        public AuthenticationDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Type).NotEmpty().Equal("OIDC");
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Uri).NotEmpty().SetValidator(new UriValidator());
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecret).NotNull()
                .SetValidator(new ClientSecretDtoValidator());
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