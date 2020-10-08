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

        private readonly JObject _jsonObjectFileMalformedVarsAndParams = JObject.Parse(@"
        {
            ""eventName"": ""eshopworld.someeventname"",
            ""subscriberName"": ""retailers"",
            ""webhooks"": {
                ""selectionRule"": ""$.TenantCode"",
                ""payloadTransform"": ""Response"",
                ""endpoints"": [
                    {
                        ""uri"": ""{vars:}{params:}/api/v2.0/WebHook/ClientOrderFailureMethod"",
                        ""selector"": ""*"",
                        ""authentication"": ""{vars:sts-settings}"",
                        ""httpVerb"": ""POST""
                    },
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
            result.ShouldHaveAnyValidationError().WithErrorMessage("File must declare variable 'uri' for the requested environment.");
        }

        [Fact, IsUnit]
        public void Validate_When_VariableIsMalformed_Then_ErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>()
            {
                {  "path", JToken.Parse("\"thepath\"") },
                {  "sts-settings", JToken.Parse("{ \"type\": \"None\" }") },
            };
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFileMalformedVarsAndParams);

            // Assert
            result.ShouldHaveAnyValidationError().WithErrorMessage("'Variable' must not be empty.");
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
                .WithErrorMessage("File must declare variable 'uri' for the requested environment.")
                .WithErrorMessage("File must declare variable 'path' for the requested environment.")
                .WithErrorMessage("File must declare variable 'sts-settings' for the requested environment.");
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

        [Fact, IsUnit]
        public void Validate_When_ParameterIsMalformed_Then_ErrorIsReturned()
        {
            // Arrange
            var replacements = new Dictionary<string, string>();
            var validator = new FileReplacementsValidator(replacements);

            // Act
            var result = validator.TestValidate(_jsonObjectFileMalformedVarsAndParams);

            // Assert
            result.ShouldHaveAnyValidationError().WithErrorMessage("'Parameter' must not be empty.");
        }
    }
}
