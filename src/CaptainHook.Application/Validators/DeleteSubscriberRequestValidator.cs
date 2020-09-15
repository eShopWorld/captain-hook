using CaptainHook.Application.Requests.Subscribers;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class DeleteSubscriberRequestValidator : AbstractValidator<DeleteSubscriberRequest>
    {
        public DeleteSubscriberRequestValidator()
        {
            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
        }
    }
}