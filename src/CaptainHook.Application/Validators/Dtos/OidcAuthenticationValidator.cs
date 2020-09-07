using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class OidcAuthenticationValidator : AuthenticationDtoValidator<OidcAuthenticationDto>
    {
        public OidcAuthenticationValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.ClientId).NotEmpty();
            RuleFor(x => x.Uri).NotEmpty().SetValidator(new UriValidator(false));
            RuleFor(x => x.Scopes).NotEmpty();
            RuleForEach(x => x.Scopes).NotEmpty();
            RuleFor(x => x.ClientSecretKeyName).NotEmpty();
        }
    }
}
