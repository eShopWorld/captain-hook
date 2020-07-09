using FluentValidation;

namespace CaptainHook.Domain.RequestValidators
{
    public class AddSubscriberRequestValidator : AbstractValidator<AddSubscriberRequest>
    {
        public AddSubscriberRequestValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("foobar").WithMessage("Provide a Name, please!");
            RuleFor(x => x.EventName).Length(2, 10);
        }
    }
}
