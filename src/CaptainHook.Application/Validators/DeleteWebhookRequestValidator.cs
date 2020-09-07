using CaptainHook.Application.Requests.Subscribers;
using FluentValidation;

namespace CaptainHook.Application.Validators
{
    public class DeleteWebhookRequestValidator : AbstractValidator<DeleteWebhookRequest>
    {
        public DeleteWebhookRequestValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.EventName).NotEmpty();
            RuleFor(x => x.SubscriberName).NotEmpty();
            RuleFor(x => x.Selector).NotEmpty();
        }
    }
}