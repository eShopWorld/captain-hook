using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class AuthenticationDtoValidator<T> : AbstractValidator<T> where T : AuthenticationDto
    {
        public AuthenticationDtoValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.Type).NotEmpty();
        }
    }
}