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
                {"sts-settings", JToken.Parse(@"{ ""type"": ""OIDC"" }")}
            };

            var source = @"{ ""authentication"": ""{vars:sts-settings}"" }";

            // Act
            var result = _jsonVarsValuesReplacer.Replace("vars", source, replacements);
            
            // Assert
            var expected = new OperationResult<string>(@"{ ""authentication"": {
  ""type"": ""OIDC""
} }");
            result.Should().BeEquivalentTo(expected);
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
            var objValue = JToken.Parse(@"{""type"": ""OIDC""}");
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla",
                ["obj-var1"] = objValue
            };

            var result = _jsonVarsValuesReplacer.Replace("vars", SimpleJsonWithVars, varsDictionary);

            const string expectedString = @"
{  
    ""eventName"": ""event"",
    ""objectType"": {
  ""type"": ""OIDC""
},
    ""valueType"": ""blah.bla"",
    ""inlineValueType"": ""https://blah.bla/api""
}";
            result.Should().BeEquivalentTo(new OperationResult<string>(expectedString));
        }

        [Fact, IsUnit]
        public void Replace_MultilevelJsonTemplate_ReturnsValidString()
        {
            var objValue = JToken.Parse(@"{""type"": ""OIDC""}");
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
            const string expectedString = @"{
    ""eventName"": ""event"",
    ""objectType"": {
  ""type"": ""OIDC""
},
    ""valueType"": {
        ""nestedValue"":""blah.bla""
    },
    ""inlineValueType"": ""blah.bla""
}";
            var expected = new OperationResult<string>(expectedString);

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
        public void Replace_MultipleUndeclaredVariableSameReplacementType_ReturnsError()
        {
            var varsDictionary = new Dictionary<string, JToken>();

            var result = _jsonVarsValuesReplacer.Replace("vars", SimpleJsonWithVars, varsDictionary);
            var expected = new OperationResult<string>(new CliExecutionError("Template has undeclared vars: obj-var1, val-var1"));
            result.Should().BeEquivalentTo(expected);
        }

        [Fact, IsUnit]
        public void Replace_UndeclaredVariableDifferentReplacementType_ReturnsOriginalString()
        {
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["val-var1"] = "blah.bla"
            };

            var result = _jsonVarsValuesReplacer.Replace("int", SimpleJsonWithVars, varsDictionary);
            result.Should().BeEquivalentTo(new OperationResult<string>(SimpleJsonWithVars));
        }

        [Theory, IsUnit]
        [InlineData("lowercase")]
        [InlineData("UPPERCASE")]
        [InlineData("MiXeDcAsE")]
        [InlineData("123")]
        [InlineData("_")]
        [InlineData("12ab")]
        [InlineData("ab12")]
        public void Replace_DifferentReplacementTypes_ReturnsOriginalString(string replacementPrefix)
        {
            var varsDictionary = new Dictionary<string, JToken>
            {
                ["strValue"] = "Lo"
            };
            var template = $@"{{ ""type"": ""Hi, {{{replacementPrefix}:strValue}}"" }}";
            var result = _jsonVarsValuesReplacer.Replace(replacementPrefix, template, varsDictionary);

            result.Should().BeEquivalentTo(new OperationResult<string>(@"{ ""type"": ""Hi, Lo"" }"));
        }

        [Theory, IsUnit]
        [InlineData("(")]
        [InlineData(" ")]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("$ab")]
        [InlineData("*t")]
        public void Replace_InvalidReplacementTypes_ReturnsError(string replacementPrefix)
        {
            var varsDictionary = new Dictionary<string, JToken>();
            var result = _jsonVarsValuesReplacer.Replace(replacementPrefix, SimpleJsonWithVars, varsDictionary);

            result.IsError.Should().BeTrue();
        }

        [Fact, IsUnit]
        public void Replace_NonJsonTemplate_SuccessfulReplacement()
        {
            var result = _jsonVarsValuesReplacer.Replace("param", "Hi, {param:person}!", new Dictionary<string, JToken>{["person"] = "Captain Hook" });
            result.Should().BeEquivalentTo(new OperationResult<string>("Hi, Captain Hook!"));
        }

        [Theory, IsUnit]
        [InlineData("{param:value")]
        [InlineData("{param: value")]
        [InlineData("param:value}")]
        [InlineData("{param;value}")]
        [InlineData("{param:va lue}")]
        public void Replace_MalformedVariableReplacement_ReturnsOriginalString(string variableTemplate)
        {
            var result = _jsonVarsValuesReplacer.Replace("param", $@"{{ ""type"": Hi, {variableTemplate} }}", new Dictionary<string, JToken> { ["value"] = "ValidValue" });
            result.Should().BeEquivalentTo(new OperationResult<string>($@"{{ ""type"": Hi, {variableTemplate} }}"));
        }

        [Fact, IsUnit]
        public void Replace_ObjectAtBoundary_ReturnsError()
        {
            var result = _jsonVarsValuesReplacer.Replace("param", "{param:value}", 
                new Dictionary<string, JToken> { ["value"] = JToken.Parse(@"{""prop"": ""val""}") });
            result.Should().BeEquivalentTo(
                new OperationResult<string>(new CliExecutionError("Invalid template at param usage '{param:value}'.")));
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
