﻿using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace CaptainHook.Domain.Entities
{
    public class WebhooksEntityValidator : AbstractValidator<WebhooksEntity>
    {
        public WebhooksEntityValidator()
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Endpoints)
                .SetValidator(new EndpointsCollectionValidator());

            RuleFor(x => x)
                .Must(ContainOnlyDefaultEndpointIfNoSelectionRule)
                .WithMessage("Only a single default endpoint is allowed if no selection rule provided");

            RuleFor(x => x.UriTransform)
                .SetValidator((webhooksEntity, uriTransform) => new UriTransformValidator(webhooksEntity.Endpoints))
                    .When(x => x.UriTransform?.Replace != null, ApplyConditionTo.CurrentValidator);
        }

        private bool ContainOnlyDefaultEndpointIfNoSelectionRule(WebhooksEntity webhooks)
        {
            return !string.IsNullOrEmpty(webhooks.SelectionRule) || ContainOnlySingleWebhookWithDefaultSelector(webhooks.Endpoints);
        }

        private static bool ContainOnlySingleWebhookWithDefaultSelector(IEnumerable<EndpointEntity> endpoints)
        {
            return endpoints?.Count(e => EndpointEntity.IsDefaultSelector(e.Selector)) == 1 && endpoints?.Count() == 1;
        }
    }
}