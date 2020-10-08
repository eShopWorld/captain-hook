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
    public class JsonVarsExtractorTests
    {
        private readonly JsonVarsExtractor _jsonVarsExtractor;

        public JsonVarsExtractorTests()
        {
            _jsonVarsExtractor = new JsonVarsExtractor();
        }

        [Fact, IsUnit]
        public void ExtractVars_WhenVarsObjectNull_ReturnsEmptyDictionary()
        {
            var result = _jsonVarsExtractor.ExtractVars(null, "CI");
            result.Should()
                .BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(new Dictionary<string, JToken>()));
        }

        [Theory, IsUnit]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("SeeEye")]
        public void ExtractVars_WhenEnvironmentNull_ReturnsError(string env)
        {
            var result = _jsonVarsExtractor.ExtractVars(JObject.FromObject(new object()), env);
            result.Should()
                .BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(
                    new CliExecutionError($"Cannot extract vars for environment '{env}'.")));
        }

        [Theory, IsUnit]
        [InlineData("ci", "https://ci-sample.url/api/doSomething")]
        [InlineData("test", "https://test-sample.url/api/doSomething")]
        [InlineData("prep", "https://prep-sample.url/api/doSomething")]
        [InlineData("sand", "https://sand-sample.url/api/doSomething")]
        [InlineData("prod", "https://prod-sample.url/api/doSomething")]
        public void ExtractVars_DifferentEnvironment_ReturnsCorrectValue(string env, string value)
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""ci"": ""https://ci-sample.url/api/doSomething"",
                    ""test"": ""https://test-sample.url/api/doSomething"",
                    ""prep"": ""https://prep-sample.url/api/doSomething"",
                    ""sand"": ""https://sand-sample.url/api/doSomething"",
                    ""prod"": ""https://prod-sample.url/api/doSomething""    
                }
            }");

            var result = _jsonVarsExtractor.ExtractVars(input, env);
            result.Should().BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(
                new Dictionary<string, JToken>
                {
                    ["retailer-url"] = value
                }));
        }

        [Fact, IsUnit]
        public void ExtractVars_MultipleVariablesInJson_ReturnsCorrectValues()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""prod"": ""https://prod-sample.url/api/doSomething"",
                    ""ci"": ""https://ci-sample.url/api/doSomething""
                },
                ""the-other-var"": {
                    ""ci"": ""ci-value"",
                    ""prep"": ""prep-value""
                }
            }");

            var result = _jsonVarsExtractor.ExtractVars(input, "ci");
            result.Should().BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(
                new Dictionary<string, JToken>
                {
                    ["retailer-url"] = "https://ci-sample.url/api/doSomething",
                    ["the-other-var"] = "ci-value"
                }));
        }

        [Fact, IsUnit]
        public void ExtractVars_MultipleVariablesCsvEnvironmentsInJson_ReturnsCorrectValue()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""prod,prep"": ""https://prod-sample.url/api/doSomething"",
                    ""ci"": ""https://ci-sample.url/api/doSomething""
                },
                ""the-other-var"": {
                    ""ci,prep"": ""ci-value"",
                    ""prep"": ""prep-value""
                }
            }");

            var result = _jsonVarsExtractor.ExtractVars(input, "prep");
            result.Should().BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(
                new Dictionary<string, JToken>
                {
                    ["retailer-url"] = "https://prod-sample.url/api/doSomething",
                    ["the-other-var"] = "ci-value"
                }));
        }

        [Fact, IsUnit]
        public void ExtractVars_NoVarForEnv_ReturnsEmptyDictionary()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""prod"": ""https://prod-sample.url/api/doSomething"",
                    ""ci"": ""https://ci-sample.url/api/doSomething""
                },
                ""the-other-var"": {
                    ""ci"": ""ci-value"",
                    ""prep"": ""prep-value""
                }
            }");

            var result = _jsonVarsExtractor.ExtractVars(input, "test");
            result.Should()
                .BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(new Dictionary<string, JToken>()));
        }

        [Fact, IsUnit]
        public void Extract_InvalidEnvironmentInJObject_ReturnsError()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""bob"": ""https://prod-sample.url/api/doSomething"",
                },
                ""the-other-var"": {
                    ""ci"": ""prep-value""
                }
            }");

            var result = _jsonVarsExtractor.ExtractVars(input, "test");
            result.Should().BeEquivalentTo(new OperationResult<Dictionary<string, JToken>>(
                new CliExecutionError("Unsupported environment 'bob' while parsing vars.")));
        }

        [Fact, IsUnit]
        public void Extract_VarsWithoutValuesObject_ReturnsError()
        {
            var input = JObject.Parse(@"{ ""prod"": ""prod-value"" }");
            var result = _jsonVarsExtractor.ExtractVars(input, "ci");
            result.IsError.Should().BeTrue();
            result.Error.Message.Should().StartWith(@"Cannot parse vars");
        }
    }

}