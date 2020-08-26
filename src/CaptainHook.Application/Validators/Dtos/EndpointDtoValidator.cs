using CaptainHook.Application.Validators.Common;
using CaptainHook.Contract;
using FluentValidation;
using System.Security.Cryptography.X509Certificates;

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
                .WithMessage($"Authentication type must be one of these values: {BasicAuthenticationDto.Type}, {OidcAuthenticationDto.Type}.")
                .SetValidator(new PolymorphicValidator<EndpointDto, AuthenticationDto>()
                    .Add(new BasicAuthenticationValidator())
                    .Add(new OidcAuthenticationValidator()));
        }
    }
}