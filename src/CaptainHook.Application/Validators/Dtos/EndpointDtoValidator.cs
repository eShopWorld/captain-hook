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
                .Must(x => !(x is InvalidAuthenticationDto))
                .WithMessage($"Authentication type must be one of these values: {NoAuthenticationDto.Type}, {BasicAuthenticationDto.Type}, {OidcAuthenticationDto.Type}.")
                .SetValidator(new PolymorphicValidator<EndpointDto, AuthenticationDto>()
                    .Add(new BasicAuthenticationDtoValidator())
                    .Add(new OidcAuthenticationDtoValidator()));
        }
    }
}