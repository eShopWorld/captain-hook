using System.Collections.Generic;
using System.Linq;
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
            RuleFor(x => x.Scopes).Must(BeSetAccordingToUseHeaders)
                 .WithMessage("'Scopes' must be defined only if 'UseHeaders' is false and must be not defined if 'UseHeaders' is true");
            RuleForEach(x => x.Scopes).NotEmpty().When(x => !x.UseHeaders);
            RuleFor(x => x.ClientSecretKeyName).NotEmpty()
                .WithMessage("'ClientSecretKeyName' must not be empty.");
            RuleFor(x => x.Uri).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new UriValidator(false));
        }

        private bool BeSetAccordingToUseHeaders(OidcAuthenticationDto authDto, List<string> scopes)
        {
            return authDto.UseHeaders ? scopes == null || !scopes.Any() : scopes != null && scopes.Any();
        }
    }
}
