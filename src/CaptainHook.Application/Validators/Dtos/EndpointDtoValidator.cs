using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class EndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public EndpointDtoValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.HttpVerb).NotEmpty()
                .SetValidator(new HttpVerbValidator());
            RuleFor(x => x.Uri).NotEmpty()
                .SetValidator(new UriValidator());
            RuleFor(x => x.Authentication).NotNull()
                .SetValidator(new PolymorphicValidator<EndpointDto, AuthenticationDto>()
                    .Add(new BasicAuthenticationValidator())
                    .Add(new OidcAuthenticationValidator()));
        }
    }
}