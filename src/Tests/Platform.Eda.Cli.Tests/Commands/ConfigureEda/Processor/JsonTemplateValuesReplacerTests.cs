using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class JsonTemplateValuesReplacerTests
    {
        private readonly JsonTemplateValuesReplacer _jsonVarsValuesReplacer;

        public JsonTemplateValuesReplacerTests()
        {
            _jsonVarsValuesReplacer = new JsonTemplateValuesReplacer();
        }

        [Fact, IsUnit]
        public void Replace_ReplacementIsObject_ReplacedWithObject()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                {"sts-settings", JToken.Parse("{ \"type\": \"OIDC\" } ")},
            };

            var source = @"{ ""authentication"": ""{vars:sts-settings}"" }";

            // Act
            var result = _jsonVarsValuesReplacer.Replace("vars", source, replacements);

            // Assert
            result.Should().BeEquivalentTo(
                    new OperationResult<string>($@"{{ ""authentication"": {JToken.Parse("{ \"type\": \"OIDC\" } ")} }}"));
        }

        [Fact, IsUnit]
        public void Replace_ReplacementIsObjectButTemplateIsValue_ReturnsError()
        {
            // Arrange
            var replacements = new Dictionary<string, JToken>
            {
                {"sts-settings", JToken.Parse("{ \"type\": \"OIDC\" } ")},
            };

            var source = @" { ""uri"": ""Here's the reference {vars:sts-settings} object."" }";

            // Act
            var result = _jsonVarsValuesReplacer.Replace("vars", source, replacements);

            // Assert
            result.Should().BeEquivalentTo(new OperationResult<string>(
                new CliExecutionError("vars replacement error. 'sts-settings' is defined as an object but used as value.")));
        }

        [Fact, IsUnit]
        public void Replace_VarObjectAndValReplacements_ReturnsValidString()
        {
            var objValue = JToken.Parse(@"{""clientSecretKeyName"": ""secret-key""}");
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla",
                ["obj-var1"] = objValue
            };

            var result = _jsonVarsValuesReplacer.Replace("vars", SimpleJsonWithVars, varsDictionary);

            result.Should().BeEquivalentTo(new OperationResult<string>($@"
{{  
    ""eventName"": ""event"",
    ""objectType"": {objValue},
    ""valueType"": ""blah.bla"",
    ""inlineValueType"": ""https://blah.bla/api""
}}"));
        }

        [Fact, IsUnit]
        public void Replace_MultilevelJsonTemplate_ReturnsValidString()
        {
            var objValue = JToken.Parse(@"{""clientSecretKeyName"": ""secret-key""}");
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla",
                ["obj-var1"] = objValue
            };

            var json = @"{
    ""eventName"": ""event"",
    ""objectType"": ""{vars:obj-var1}"",
    ""valueType"": {
        ""nestedValue"":""{vars:val-var1}""
    },
    ""inlineValueType"": ""{vars:val-var1}""
}";

            var result = _jsonVarsValuesReplacer.Replace("vars", json, varsDictionary);
            var expected = new OperationResult<string>($@"{{
    ""eventName"": ""event"",
    ""objectType"": {objValue},
    ""valueType"": {{
        ""nestedValue"":""blah.bla""
    }},
    ""inlineValueType"": ""blah.bla""
}}");

            result.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void Replace_UndeclaredVariableSameReplacementType_ReturnsError()
        {
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla",
            };

            var result = _jsonVarsValuesReplacer.Replace("vars", SimpleJsonWithVars, varsDictionary);
            var expected = new OperationResult<string>(new CliExecutionError("Template has undeclared vars: obj-var1"));
            result.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void Replace_UndeclaredVariableDifferentReplacementType_ReturnsValidString()
        {
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla",
            };

            var result = _jsonVarsValuesReplacer.Replace("int", SimpleJsonWithVars, varsDictionary);
            result.Should().BeEquivalentTo(new OperationResult<string>(SimpleJsonWithVars));
        }

        [Theory, IsUnit]
        [InlineData("Nothing")]
        [InlineData("var")]
        [InlineData("123")]
        public void Replace_RandomReplacementTypes_ReturnsFileContentUnchanged(string replacementPrefix)
        {
            var varsDictionary = new Dictionary<string, JToken>();
            var result = _jsonVarsValuesReplacer.Replace(replacementPrefix, SimpleJsonWithVars, varsDictionary);

            result.IsError.Should().BeFalse();
            result.Data.Should().Be(SimpleJsonWithVars);
        }

        [Theory, IsUnit]
        [InlineData("(")]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        public void Replace_InvalidReplacementTypes_ReturnsError(string replacementPrefix)
        {
            var varsDictionary = new Dictionary<string, JToken>();
            var result = _jsonVarsValuesReplacer.Replace(replacementPrefix, SimpleJsonWithVars, varsDictionary);

            result.IsError.Should().BeTrue();
        }

        private static string SimpleJsonWithVars => @"
{  
    ""eventName"": ""event"",
    ""objectType"": ""{vars:obj-var1}"",
    ""valueType"": ""{vars:val-var1}"",
    ""inlineValueType"": ""https://{vars:val-var1}/api""
}";


    }
}
