using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class OidcAuthenticationValidator : AuthenticationDtoValidator<OidcAuthenticationDto>
    {
        public OidcAuthenticationValidator()
        {
            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecretKeyName).NotEmpty();
            RuleFor(x => x.Uri).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new UriValidator(false));
        }
    }
}
