using CaptainHook.Application.Requests.Subscribers;
using FluentValidation;

namespace CaptainHook.Application.RequestValidators
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
