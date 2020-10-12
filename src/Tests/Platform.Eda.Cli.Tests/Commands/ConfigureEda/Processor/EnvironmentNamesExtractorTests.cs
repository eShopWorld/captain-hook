using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Results;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class EnvironmentNamesExtractorTests
    {
        [Fact, IsUnit]
        public void Find_WhenVarsObjectNull_ReturnsEmptyCollection()
        {
            var result = EnvironmentNamesExtractor.FindInVars(null);
            result.Should()
                .BeEquivalentTo(new OperationResult<IEnumerable<string>>(Enumerable.Empty<string>()));
        }

        [Fact, IsUnit]
        public void Find_MultipleEnvironmentsForOneVariable_ReturnsAllNamesInLowerCase()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""Ci"": ""https://ci-sample.url/api/doSomething"",
                    ""tESt"": ""https://test-sample.url/api/doSomething"",
                    ""PREP"": ""https://prep-sample.url/api/doSomething"",
                    ""sand"": ""https://sand-sample.url/api/doSomething"",
                    ""prod"": ""https://prod-sample.url/api/doSomething""    
                }
            }");

            var result = EnvironmentNamesExtractor.FindInVars(input);

            result.Data.Should().BeEquivalentTo("ci", "test", "prep", "sand", "prod");
        }

        [Fact, IsUnit]
        public void Find_MultipleVariablesInJson_ReturnsEnvironmentsFromAllVariables()
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

            var result = EnvironmentNamesExtractor.FindInVars(input);

            result.Data.Should().BeEquivalentTo("prod", "ci", "prep");
        }

        [Fact, IsUnit]
        public void Find_MultipleVariablesCsvEnvironmentsInJson_ReturnsEnvironmentsFromAllVariables()
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

            var result = EnvironmentNamesExtractor.FindInVars(input);

             result.Data.Should().BeEquivalentTo("prod", "prep", "ci");
        }

        [Fact, IsUnit]
        public void Find_InvalidEnvironmentInJObject_ReturnsError()
        {
            var input = JObject.Parse(@"{
                ""retailer-url"": { 
                    ""bob"": ""https://prod-sample.url/api/doSomething"",
                },
                ""the-other-var"": {
                    ""foo,bar"": ""prep-value""
                }
            }");

            var result = EnvironmentNamesExtractor.FindInVars(input);

            result.Error.Should().BeEquivalentTo(new CliExecutionError("File contains unknown envs names: 'bob,foo,bar'."));
        }

        [Fact, IsUnit]
        public void Find_EnvsWithoutValuesObject_ReturnsError()
        {
            var input = JObject.Parse(@"{ ""prod"": ""prod-value"" }");

            var result = EnvironmentNamesExtractor.FindInVars(input);
            
            result.IsError.Should().BeTrue();
            result.Error.Message.Should().StartWith(@"Cannot parse vars");
        }
    }
}