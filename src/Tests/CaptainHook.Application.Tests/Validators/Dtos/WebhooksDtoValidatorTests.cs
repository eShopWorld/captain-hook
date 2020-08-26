using System.Collections.Generic;
using CaptainHook.Application.Validators.Dtos;
using CaptainHook.Contract;
using Eshopworld.Tests.Core;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Application.Tests.Validators.Dtos
{
    public class WebhooksDtoValidatorTests
    {
        private readonly IValidator<WebhooksDto> _validator = new WebhooksDtoValidator();

        [Fact, IsUnit]
        public void Validate_NullCollection_CollectionIsNotValid()
        {
            var webhooksDto = new WebhooksDto();

            var result = _validator.TestValidate(webhooksDto);

            result.ShouldHaveValidationErrorFor(x => x.Endpoints)
                .WithErrorMessage("Webhooks list must contain at least one endpoint");
        }

        [Fact, IsUnit]
        public void Validate_EmptyCollection_CollectionIsNotValid()
        {
            var webhooksDto = new WebhooksDto { Endpoints = new List<EndpointDto>(0) };

            var result = _validator.TestValidate(webhooksDto);

            result.ShouldHaveValidationErrorFor(x => x.Endpoints)
                .WithErrorMessage("Webhooks list must contain at least one endpoint");
        }
    }
}