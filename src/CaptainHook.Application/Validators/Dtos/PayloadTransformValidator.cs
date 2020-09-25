using CaptainHook.Contract;
using FluentValidation;
using System;
using System.Linq;

namespace CaptainHook.Application.Validators.Dtos
{
    public class PayloadTransformValidator : AbstractValidator<string>
    {
        private static readonly string[] AllowedTransformTypes = { "Request", "Response", "OrderConfirmation", "PlatformOrderConfirmation", "EmptyCart" };

        public PayloadTransformValidator(WebhooksValidatorDtoType subject)
        {
            RuleFor(x => x)
                .Null()
                .WithMessage("Payload Transformation is not allowed for callbacks")
                .When(_ => subject == WebhooksValidatorDtoType.Callback);

            RuleFor(x => x)
                .Must(BeValidPayloadTransformType)
                .WithMessage($"Values allowed for PayloadTransform are: {string.Join(", ", AllowedTransformTypes)}")
                .When(_ => subject != WebhooksValidatorDtoType.Callback);
        }

        private static bool BeValidPayloadTransformType(string payloadTransform)
        {
            return
                string.IsNullOrEmpty(payloadTransform) ||
                AllowedTransformTypes.Any(x => x.Equals(payloadTransform, StringComparison.OrdinalIgnoreCase));
        }
    }
}
