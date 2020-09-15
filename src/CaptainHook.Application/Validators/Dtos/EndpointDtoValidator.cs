using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class EndpointDtoValidator : AbstractValidator<EndpointDto>
    {
        public EndpointDtoValidator()
        {
            RuleFor(x => x.HttpVerb).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new HttpVerbValidator());
            RuleFor(x => x.Uri).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .SetValidator(new UriValidator());
            RuleFor(x => x.Authentication).Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage($"Authentication type must be one of these values: {BasicAuthenticationDto.Type}, {OidcAuthenticationDto.Type}.")
                .SetValidator(new PolymorphicValidator<EndpointDto, AuthenticationDto>()
                    .Add(new BasicAuthenticationValidator())
                    .Add(new OidcAuthenticationValidator()));
        }
    }
}