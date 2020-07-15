using CaptainHook.Common.Configuration;
using FluentValidation;

namespace CaptainHook.EventHandlerActor.Validation
{
    public class WebhookConfigForRouteAndReplaceValidator : AbstractValidator<WebhookConfig>
    {
        public WebhookConfigForRouteAndReplaceValidator()
        {
            
        }
    }
}
