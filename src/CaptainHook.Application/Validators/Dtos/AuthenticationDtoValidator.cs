using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class AuthenticationDtoValidator<T> : AbstractValidator<T> where T : AuthenticationDto
    {
    }
}