using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using FluentValidation;
using Newtonsoft.Json.Linq;
using UriTransformValidator = CaptainHook.Application.Validators.Common.UriTransformValidator;

namespace CaptainHook.Application.Validators.Dtos
{
    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
    {
        private static readonly JObject _jObject = new JObject();

        private static readonly string[] AllowedTransformTypes = { "Request", "Response", "OrderConfirmation", "PlatformOrderConfirmation", "EmptyCart" };

        public WebhooksDtoValidator(WebhooksDtoType subject)
        {
            RuleFor(x => x.SelectionRule).Cascade(CascadeMode.Stop)
                .NotEmpty()
                .Must(BeValidJsonPathExpression)
                    .WithMessage("The SelectionRule must be a valid JSONPath expression")
                .When(ThereIsAtLeastOneEndpointWithSelectorDefined);

            RuleFor(x => x.Endpoints).Cascade(CascadeMode.Stop)
                .NotNull()
                .WithMessage($"{subject} list must contain at least one endpoint")
                .NotEmpty()
                .WithMessage($"{subject} list must contain at least one endpoint")
                .Must(ContainAtMostOneEndpointWithDefaultSelector)
                    .WithMessage("There can be only one endpoint with the default selector")
                .Must(NotContainMultipleEndpointsWithTheSameSelector)
                    .WithMessage("There cannot be multiple endpoints with the same selector");

            RuleFor(x => x.PayloadTransform)
                .Null()
                .WithMessage("Payload Transformation is not allowed for callbacks")
                .When(_ => subject == WebhooksDtoType.Callback);

            RuleFor(x => x.PayloadTransform)
                .Must(BeValidPayloadTransformType)
                .WithMessage($"Values allowed for PayloadTransform are: {string.Join(", ", AllowedTransformTypes)}")
                .When(_ => subject != WebhooksDtoType.Callback);

            RuleForEach(x => x.Endpoints).Cascade(CascadeMode.Stop)
                .SetValidator(new EndpointDtoValidator());

            RuleFor(x => x.UriTransform).Cascade(CascadeMode.Stop)
                .SetValidator((webhooksDto, uriTransform) => new UriTransformValidator(webhooksDto.Endpoints))
                    .When(x => x.UriTransform?.Replace != null, ApplyConditionTo.CurrentValidator);
        }

        private static bool BeValidPayloadTransformType(string payloadTransform)
        {
            return 
                string.IsNullOrEmpty(payloadTransform) || 
                AllowedTransformTypes.Any(x => x.Equals(payloadTransform, StringComparison.OrdinalIgnoreCase));
        }

        private static bool ThereIsAtLeastOneEndpointWithSelectorDefined(WebhooksDto webhooks)
        {
            return webhooks.Endpoints?.Any(x => !EndpointEntity.IsDefaultSelector(x.Selector)) ?? false;
        }

        private static bool ContainAtMostOneEndpointWithDefaultSelector(List<EndpointDto> endpoints)
        {
            return endpoints?.Count(x => EndpointEntity.IsDefaultSelector(x.Selector)) <= 1;
        }

        private static bool NotContainMultipleEndpointsWithTheSameSelector(List<EndpointDto> endpoints)
        {
            return !(endpoints ?? Enumerable.Empty<EndpointDto>())
                .GroupBy(x => x.Selector)
                .Any(x => x.Count() > 1);
        }

        private static bool BeValidJsonPathExpression(string selectionRule)
        {
            if (!selectionRule.StartsWith('$'))
            {
                return false;
            }

            try
            {
                _jObject.SelectToken(selectionRule, false);
            }
            catch
            {
                return false;
            }

            return true;
        }
    }
}