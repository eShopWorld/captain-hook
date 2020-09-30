using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class BasicAuthenticationDtoValidator : AuthenticationDtoValidator<BasicAuthenticationDto>
    {
        public BasicAuthenticationDtoValidator()
        {
            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.PasswordKeyName).NotEmpty()
                .WithMessage("'PasswordKeyName' must not be empty.");
        }
    }
}
