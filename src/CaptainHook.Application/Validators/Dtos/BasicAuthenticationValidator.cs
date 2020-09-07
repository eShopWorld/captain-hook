﻿using CaptainHook.Contract;
using FluentValidation;

namespace CaptainHook.Application.Validators.Dtos
{
    public class BasicAuthenticationValidator : AuthenticationDtoValidator<BasicAuthenticationDto>
    {
        public BasicAuthenticationValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Username).NotEmpty();
            RuleFor(x => x.PasswordKeyName).NotEmpty();
        }
    }
}
