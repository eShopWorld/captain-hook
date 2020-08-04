using System;
using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class AuthenticationDtoValidator : AbstractValidator<AuthenticationDto>
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