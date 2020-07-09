﻿using CaptainHook.Domain.Requests;
using FluentValidation;

namespace CaptainHook.Domain.RequestValidators
{
    public class AddSubscriberRequestValidator : AbstractValidator<AddSubscriberRequest>
    {
        public AddSubscriberRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.EventName).Length(10, 100);
        }
    }
}
