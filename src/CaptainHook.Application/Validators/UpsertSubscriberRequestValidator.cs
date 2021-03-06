﻿using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators.Dtos;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class UpsertSubscriberRequestValidator : AbstractValidator<UpsertSubscriberRequest>
    {
        public UpsertSubscriberRequestValidator()
        {
            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).Cascade(CascadeMode.Stop)
               .NotEmpty()
               .MaximumLength(50);
            RuleFor(x => x.Subscriber).Cascade(CascadeMode.Stop)
                .NotNull()
                .SetValidator(new SubscriberDtoValidator());
        }
    }
}
