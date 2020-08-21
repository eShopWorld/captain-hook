using System;
using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class AuthenticationDtoValidator : AbstractValidator<OidcAuthenticationDto>
    {
        public AuthenticationDtoValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Type).NotEmpty()
                .Must(x => x.Equals("OIDC", StringComparison.OrdinalIgnoreCase))
                .WithMessage("{PropertyName} must be a valid authentication type.");
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Uri).NotEmpty().SetValidator(new UriValidator());
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecretKeyName).NotEmpty();
        }
    }
}