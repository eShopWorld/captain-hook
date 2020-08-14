using CaptainHook.Application.Requests.Subscribers;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class DeleteWebhookRequestValidator : AbstractValidator<DeleteWebhookRequest>
    {
        public DeleteWebhookRequestValidator()
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
            RuleFor(x => x.Selector).NotEmpty().When(x => x.Selector != null);
        }
    }
}