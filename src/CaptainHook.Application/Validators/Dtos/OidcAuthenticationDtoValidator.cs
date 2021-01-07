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
            
            RuleFor(x => x.Scopes)
                //.NotEmpty().When(x => !x.UseHeaders)
                .Empty().When(x => x.UseHeaders);

            RuleForEach(x => x.Scopes).NotEmpty().When(x => !x.UseHeaders);
            RuleFor(x => x.ClientSecretKeyName).NotEmpty()
                .WithMessage("'ClientSecretKeyName' must not be empty.");
            RuleFor(x => x.Uri).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new UriValidator(false));
        }
    }
}
