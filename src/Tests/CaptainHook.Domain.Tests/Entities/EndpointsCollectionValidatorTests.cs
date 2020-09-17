using System.Collections.Generic;
using CaptainHook.Domain.Entities;
using CaptainHook.TestsInfrastructure.Builders;
using Eshopworld.Tests.Core;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

namespace CaptainHook.Domain.Tests.Entities
{
    public class EndpointsCollectionValidatorTests
    {
        private IValidator<IEnumerable<EndpointEntity>> _validator;

        [Fact, IsUnit]
        public void Validate_EmptyWebhooksCollection_CollectionIsNotValid()
        {
            var endpoints = new EndpointEntity[] { };
            _validator = new EndpointsCollectionValidator(WebhooksEntityType.Webhooks);

            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Webhooks list must contain at list one endpoint");
        }

        [Fact, IsUnit]
        public void Validate_EmptyCallbacksCollection_CollectionIsValid()
        {
            var endpoints = new EndpointEntity[] { };
            _validator = new EndpointsCollectionValidator(WebhooksEntityType.Callbacks);

            var result = _validator.TestValidate(endpoints);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_MultipleEndpointsInCollection_CollectionIsValid(WebhooksEntityType type)
        {
            var endpoints = new[]
            {
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests")
                    .WithSelector(null)
                    .WithHttpVerb("POST")
                    .Create(),
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests/2")
                    .WithSelector("abc")
                    .WithHttpVerb("POST")
                    .Create()
            };

            _validator = new EndpointsCollectionValidator(type);
            var result = _validator.TestValidate(endpoints);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_MultipleEndpointsInCollectionWithoutSelector_CollectionIsNotValid(WebhooksEntityType type)
        {
            var endpoints = new[]
            {
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests")
                    .WithSelector(null)
                    .WithHttpVerb("POST")
                    .Create(),
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests/2")
                    .WithSelector(null)
                    .WithHttpVerb("POST")
                    .Create()
            };

            _validator = new EndpointsCollectionValidator(type);
            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("There cannot be multiple endpoints with the same selector");
        }

        [Theory, IsUnit]
        [InlineData(WebhooksEntityType.Webhooks)]
        [InlineData(WebhooksEntityType.Callbacks)]
        public void Validate_MultipleEndpointsInCollectionWithDuplicatedSelector_CollectionIsNotValid(WebhooksEntityType type)
        {
            var endpoints = new[]
            {
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests")
                    .WithSelector(null)
                    .WithHttpVerb("POST")
                    .Create(),
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests")
                    .WithSelector("abc")
                    .WithHttpVerb("POST")
                    .Create(),
                new EndpointBuilder()
                    .WithUri("https://uri.com/tests/2")
                    .WithSelector("abc")
                    .WithHttpVerb("POST")
                    .Create()
            };

            _validator = new EndpointsCollectionValidator(type);
            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("There cannot be multiple endpoints with the same selector");
        }
    }
}