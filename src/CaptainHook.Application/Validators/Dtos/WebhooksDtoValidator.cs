﻿using System;
using CaptainHook.Contract;
using FluentValidation;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using UriTransformValidator = CaptainHook.Application.Validators.Common.UriTransformValidator;

namespace CaptainHook.Application.Validators.Dtos
{
    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
    {
        private static readonly JObject _jObject = new JObject();

        public WebhooksDtoValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(x => x.SelectionRule).NotEmpty()
                .Must(BeValidJsonPathExpression).WithMessage("The SelectionRule must be a valid JSONPath expression")
                .When(ThereIsAtLeastOneEndpointWithSelectorDefined);

            RuleFor(x => x.Endpoints)
                .NotNull()
                .WithMessage("Webhooks list must contain at least one endpoint")
                .NotEmpty()
                .WithMessage("Webhooks list must contain at least one endpoint")
                .Must(ContainAtMostOneEndpointWithDefaultSelector)
                .WithMessage("There can be only one endpoint with the default selector")
                .Must(NotContainMultipleEndpointsWithTheSameSelector)
                .WithMessage("There cannot be multiple endpoints with the same selector");

            RuleForEach(x => x.Endpoints)
                .SetValidator(new UpsertSubscriberEndpointDtoValidator());

            RuleFor(x => x.UriTransform)
                .SetValidator((webhooksDto, uriTransform) => new UriTransformValidator(webhooksDto.Endpoints))
                    .When(x => x.UriTransform?.Replace != null, ApplyConditionTo.CurrentValidator);

        }

        private bool ThereIsAtLeastOneEndpointWithSelectorDefined(WebhooksDto webhooks)
        {
            return webhooks.Endpoints?.Any(e => e.Selector != null) ?? false;
        }

        private bool ContainAtMostOneEndpointWithDefaultSelector(List<EndpointDto> endpoints)
        {
            return endpoints?.Count(x => x.Selector != null) <= 1;
        }

        private bool NotContainMultipleEndpointsWithTheSameSelector(List<EndpointDto> endpoints)
        {
            return !(endpoints ?? Enumerable.Empty<EndpointDto>())
                .GroupBy(x => x.Selector)
                .Any(x => x.Count() > 1);
        }

        private bool BeValidJsonPathExpression(string selectionRule)
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