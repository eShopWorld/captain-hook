﻿using CaptainHook.Application.Requests.Subscribers;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class DeleteSubscriberRequestValidator : AbstractValidator<DeleteSubscriberRequest>
    {
        public DeleteSubscriberRequestValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
        }
    }
}