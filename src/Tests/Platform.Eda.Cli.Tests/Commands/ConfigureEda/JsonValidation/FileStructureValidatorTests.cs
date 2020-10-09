using System.Linq;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentValidation.TestHelper;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.JsonValidation
{
    public class FileStructureValidatorTests
    {
        private readonly FileStructureValidator validator = new FileStructureValidator();

        [Fact, IsUnit]
        public void Validate_SubscriberNameMissing_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriberName1"": ""subscriber""}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriberName").WithErrorMessage("File must contain property 'subscriberName'");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberNameIsObject_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriberName"": { ""abc"": ""value""}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriberName").WithErrorMessage("File property 'subscriberName' must be a string");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberNameEmpty_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriberName"": """"}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriberName").WithErrorMessage("File property 'subscriberName' cannot be empty");
        }

        [Fact, IsUnit]
        public void Validate_EventNameMissing_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""eventName1"": ""subscriber""}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("eventName").WithErrorMessage("File must contain property 'eventName'");
        }

        [Fact, IsUnit]
        public void Validate_EventNameEmpty_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""eventName"": """"}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("eventName").WithErrorMessage("File property 'eventName' cannot be empty");
        }

        [Fact, IsUnit]
        public void Validate_EventNameIsObject_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""eventName"": { ""abc"": ""value""}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("eventName").WithErrorMessage("File property 'eventName' must be a string");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberMissing_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber1"": ""subscriber""}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber").WithErrorMessage("File must contain property 'subscriber'");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberEmpty_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": """"}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber").WithErrorMessage("File property 'subscriber' must be an object");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberIsObject_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": ""abc""}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber").WithErrorMessage("File property 'subscriber' must be an object");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberDoesNotContainWebhooks_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": {}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber.webhooks").WithErrorMessage("File property 'subscriber' must contain property 'webhooks'");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberDoesNotContainWebhooksWithEndpoints_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": { ""webhooks"": { ""endpoints1"": []}}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber.webhooks.endpoints").WithErrorMessage("File property 'subscriber.webhooks' must contain property 'endpoints'");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberDoesNotContainWebhooksWithAtLeastOneEndpoint_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": { ""webhooks"": { ""endpoints"": []}}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber.webhooks.endpoints").WithErrorMessage("File property 'subscriber.webhooks.endpoints' must have at least a single endpoint");
        }

        [Fact, IsUnit]
        public void Validate_SubscriberDoesNotContainWebhooksWhichAreObjects_ValidationError()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": { ""webhooks"": { ""endpoints"": [""a"", {}]}}}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.ShouldHaveValidationErrorFor("subscriber.webhooks.endpoints").WithErrorMessage("File property 'subscriber.webhooks.endpoints' must have endpoints which are objects only");
        }

        [Fact, IsUnit]
        public void Validate_EmptyObject_ValidationErrorsForTopLevelProperties()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{}");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.Errors.Select(e => e.PropertyName).Should()
                .BeEquivalentTo("subscriberName", "eventName", "subscriber");
        }

        [Fact, IsUnit]
        public void Validate_EmptySubscriberObject_ValidationErrorsForTopLevelPropertiesAndSubscriberChildren()
        {
            // Arrange
            var jsonObject = JObject.Parse(@"{ ""subscriber"": {} }");

            // Act
            var result = validator.TestValidate(jsonObject);

            // Assert
            result.Errors.Select(e => e.PropertyName).Should()
                .BeEquivalentTo("subscriberName", "eventName", "subscriber.webhooks", "subscriber.webhooks.endpoints");
        }
    }
}