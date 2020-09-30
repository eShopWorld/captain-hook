using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class OidcAuthenticationDtoValidator : AuthenticationDtoValidator<OidcAuthenticationDto>
    {
        public OidcAuthenticationDtoValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty()
                .WithMessage("'ClientId' must not be empty.");
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecretKeyName).NotEmpty()
                .WithMessage("'ClientSecretKeyName' must not be empty.");
            RuleFor(x => x.Uri).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new UriValidator(false));
        }
    }
}
