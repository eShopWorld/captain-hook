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
        private readonly IValidator<IEnumerable<EndpointEntity>> _validator = new EndpointsCollectionValidator();

        [Fact, IsUnit]
        public void Validate_EmptyCollection_CollectionIsNotValid()
        {
            var endpoints = new EndpointEntity[] { };

            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("Webhooks list must contain at list one endpoint");
        }

        [Fact, IsUnit]
        public void Validate_MultipleEndpointsInCollection_CollectionIsValid()
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

            var result = _validator.TestValidate(endpoints);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void Validate_MultipleEndpointsInCollectionWithoutSelector_CollectionIsNotValid()
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

            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("There cannot be multiple endpoints with the same selector");
        }

        [Fact, IsUnit]
        public void Validate_MultipleEndpointsInCollectionWithDuplicatedSelector_CollectionIsNotValid()
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

            var result = _validator.TestValidate(endpoints);

            result.ShouldHaveValidationErrorFor(x => x)
                .WithErrorMessage("There cannot be multiple endpoints with the same selector");
        }
    }
}