using Eshopworld.Tests.Core;
using FluentValidation.TestHelper;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation;
using System.Collections.Generic;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.JsonValidation
{
    public class FileReplacementsValidatorTests
    {

        private readonly JObject _jsonObjectFile = JObject.Parse(@"
        {
            ""eventName"": ""eshopworld.someeventname"",
            ""subscriberName"": ""retailers"",
            ""webhooks"": {
            ""selectionRule"": ""$.TenantCode"",
            ""payloadTransform"": ""Response"",
            ""endpoints"": [
                {
                ""uri"": ""{vars:uri}{params:evo-host}{vars:path}/api/v2.0/WebHook/ClientOrderFailureMethod"",
                ""selector"": ""*"",
                ""authentication"": ""{vars:sts-settings}"",
                ""httpVerb"": ""POST""
                },
            ]
            },
            ""callbacks"": {
            ""selectionRule"": ""$.TenantCode"",
            ""endpoints"": [
                {
                ""uri"": ""{vars:uri}{params:evo-host}{vars:path}/api/v2/EdaResponse/ExternalEdaResponse"",
                ""selector"": ""*"",
                ""httpVerb"": ""POST""
                }
            ]
            },
            ""dlqhooks"": {
            ""selectionRule"": ""$.TenantCode"",
            ""endpoints"": [
                {
                ""uri"": ""{vars:uri}{params:evo-host}/api/v2/DlqRequest/ExternalRequest"",
                ""selector"": ""*"",
                ""httpVerb"": ""POST""
                }
            ]
            }
        }");

        [Fact, IsUnit]
        public void Validate_When_AllVariablesAreDefined_Then_NoErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>()
            {
                {  "uri", JToken.Parse("\"http://www.api.com\"") },
                {  "path", JToken.Parse("\"thepath\"") },
                {  "sts-settings", JToken.Parse("{ \"type\": \"None\" }") },
            };
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFile);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void Validate_When_VariableIsNotDefined_Then_ErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>()
            {
                {  "path", JToken.Parse("\"thepath\"") },
                {  "sts-settings", JToken.Parse("{ \"type\": \"None\" }") },
            };
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFile);

            // Assert
            result.ShouldHaveAnyValidationError().WithErrorMessage("File must declare variable 'uri'.");
        }

        [Fact, IsUnit]
        public void Validate_When_AllVariablesAreNotDefined_Then_ErrorsAreReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>();
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFile);

            // Assert
            result.ShouldHaveAnyValidationError()
                .WithErrorMessage("File must declare variable 'uri'.")
                .WithErrorMessage("File must declare variable 'path'.")
                .WithErrorMessage("File must declare variable 'sts-settings'.");
        }

        [Fact, IsUnit]
        public void Validate_When_AllParametersAreDefined_Then_NoErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, string>()
            {
                {  "evo-host", "domain.company.com" }
            };
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFile);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact, IsUnit]
        public void Validate_When_ParameterIsNotDefined_Then_ErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, string>();
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFile);

            // Assert
            result.ShouldHaveAnyValidationError().WithErrorMessage("CLI run must provide parameter 'evo-host'.");
        }
    }
}
