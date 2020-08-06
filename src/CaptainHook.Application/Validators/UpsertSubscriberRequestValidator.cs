using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Validators.Dtos;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class UpsertSubscriberRequestValidator : AbstractValidator<UpsertSubscriberRequest>
    {
        public UpsertSubscriberRequestValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
            RuleFor(x => x.Subscriber).NotNull()
                .SetValidator(new SubscriberDtoValidator());
        }
    }
}
