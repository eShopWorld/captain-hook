using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;
using Moq;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Xunit;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda.Processor
{
    public class PutSubscriberProcessChainTests
    {
        private static readonly CliExecutionError CliExecutionError = new CliExecutionError("CLI Error");
        private static readonly Dictionary<string, string> EmptyReplacementsDictionary = new Dictionary<string, string>();

        private readonly Mock<ISubscribersDirectoryProcessor> _subscribersDirectoryProcessorMock;
        private readonly Mock<ISubscriberFileParser> _subscriberFileParserMock;
        private readonly Mock<IJsonVarsExtractor> _jsonVarsExtractorMock;
        private readonly Mock<IJsonTemplateValuesReplacer> _jsonTemplateValuesReplacerMock;
        private readonly Mock<IApiConsumer> _apiConsumerMock;

        private readonly PutSubscriberProcessChain _sut;
        private readonly Mock<TextWriter> _errorWriter;
        private readonly Mock<TextWriter> _outWriter;

        private static readonly string _validSubscriberRequestContent = @"
              ""eventName"": ""eshopworld.platform.events.oms.lineitemcancelsucceededevent"",
              ""subscriberName"": ""{params:subname}"",
              ""subscriber"": {
                ""webhooks"": {
                  ""selectionRule"": ""$.TenantCode"",
                  ""payloadTransform"": ""Response"",
                  ""endpoints"": [
                    {
                      ""uri"": ""{vars:invoicing-url}"",
                      ""selector"": ""*"",
                      ""authentication"": { ""type"": ""None"" },
                      ""httpVerb"": ""POST""
                    }
                  ]
                }
              }
        ";

        private static readonly string ValidSubscriberRequest = $"{{{_validSubscriberRequestContent}}}";

        private static readonly JObject ValidSubscriberFileJson = JObject.Parse(@$"
            {{
              ""vars"": {{
                ""invoicing-url"": {{
                  ""ci"": ""https://log1-randomapi.eshopworld.net/api/v2.0/MethodOne/ActionItem"",
                  ""test"": ""https://re1-randomapi.eshopworld.net/api/v2.0/MethodOne/ActionItem"",
                  ""prep"": ""https://perf-randomapi.eshopworld.net/api/v2.0/MethodOne/ActionItem"",
                  ""sand"": ""https://uatrc-randomapi.eshopworld.net/api/v2.0/MethodOne/ActionItem"",
                  ""prod"": ""https://randomapi.eshopworld.com/api/v2.0/MethodOne/ActionItem""
                }}
              }},
              {_validSubscriberRequestContent}
            }}");

        private static async IAsyncEnumerable<ApiOperationResult> EmptyAsyncEnumerable()
        {
            yield return new ApiOperationResult
            {
                File = new FileInfo("file1.json"),
                Request = new PutSubscriberRequest(),
                Response = new HttpOperationResponse()
            };
            await Task.CompletedTask;
        }

        public PutSubscriberProcessChainTests()
        {

            _errorWriter = new Mock<TextWriter>(MockBehavior.Default);
            _outWriter = new Mock<TextWriter>(MockBehavior.Default);

            var mockConsole = new Mock<IConsole>();
            mockConsole.Setup(c => c.Error).Returns(_errorWriter.Object);
            mockConsole.Setup(c => c.Out).Returns(_outWriter.Object);

            _subscribersDirectoryProcessorMock = new Mock<ISubscribersDirectoryProcessor>();
            _subscriberFileParserMock = new Mock<ISubscriberFileParser>();
            _jsonVarsExtractorMock = new Mock<IJsonVarsExtractor>();
            _jsonTemplateValuesReplacerMock = new Mock<IJsonTemplateValuesReplacer>();
            _apiConsumerMock = new Mock<IApiConsumer>();

            IApiConsumer captainHookBuilder(string env)
            {
                return _apiConsumerMock.Object;
            }

            _sut = new PutSubscriberProcessChain(
                mockConsole.Object,
                _subscribersDirectoryProcessorMock.Object,
                _subscriberFileParserMock.Object,
                _jsonVarsExtractorMock.Object,
                _jsonTemplateValuesReplacerMock.Object,
                captainHookBuilder);
        }

        [ClassData(typeof(ValidEnvironmentNames))]
        [Theory, IsUnit]
        public async Task ProcessAsync_When_FolderIsInvalid_Then_ErrorIsReturned(string environment)
        {
            // Arrange
            var invalidFolder = "invalid folder";
            _subscribersDirectoryProcessorMock
                .Setup(x => x.ProcessDirectory(invalidFolder))
                .Returns(CliExecutionError);

            // Act
            var result = await _sut.ProcessAsync(invalidFolder, environment, EmptyReplacementsDictionary, true);

            // Assert
            using (new AssertionScope())
            {
                _subscribersDirectoryProcessorMock.Verify(x => x.ProcessDirectory(invalidFolder), Times.Once);
                VerifyWrite(_errorWriter, CliExecutionError.Message);
                result.Should().Be(1);
            }
        }

        [ClassData(typeof(ValidEnvironmentNames))]
        [Theory, IsUnit]
        public async Task ProcessAsync_WhenOneFileIsNotValidJson_Then_FileIsSkipped(string environment)
        {
            // Arrange
            _subscribersDirectoryProcessorMock
                .Setup(x => x.ProcessDirectory(It.IsAny<string>()))
                .Returns(new List<string> { "file1.json", "file2.json" });
            _subscriberFileParserMock
                .Setup(x => x.ParseFile("file1.json"))
                .Returns(JObject.FromObject(ValidSubscriberFileJson));
            _subscriberFileParserMock
                .Setup(x => x.ParseFile("file2.json"))
                .Returns(CliExecutionError);
            _jsonVarsExtractorMock
                .Setup(x => x.ExtractVars(It.IsAny<JObject>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, JToken>());
            _jsonTemplateValuesReplacerMock
                .Setup(x => x.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, JToken>>()))
                .Returns(ValidSubscriberRequest);


            _apiConsumerMock
                .Setup(x => x.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()))
                .Returns(EmptyAsyncEnumerable());

            // Act
            var result = await _sut.ProcessAsync("a folder", environment, EmptyReplacementsDictionary, true);

            // Assert
            using (new AssertionScope())
            {
                _subscriberFileParserMock.Verify(x => x.ParseFile("file1.json"), Times.Once);
                _subscriberFileParserMock.Verify(x => x.ParseFile("file2.json"), Times.Once);
                VerifyWrite(_errorWriter, CliExecutionError.Message);
                result.Should().Be(0);
            }
        }


        [Fact, IsUnit]
        public async Task ProcessAsync_VarsExtractionError_StopsProcessingAndOutputsError()
        {
            var environment = "CI";
            // Arrange
            _subscribersDirectoryProcessorMock
                .Setup(x => x.ProcessDirectory(It.IsAny<string>()))
                .Returns(new List<string> { "file1.json" });
            _subscriberFileParserMock
                .Setup(x => x.ParseFile("file1.json"))
                .Returns(JObject.FromObject(ValidSubscriberFileJson));
            _jsonVarsExtractorMock
                .Setup(x => x.ExtractVars(It.IsAny<JObject>(), It.IsAny<string>()))
                .Returns(CliExecutionError);

            // Act
            await _sut.ProcessAsync("a folder", environment, EmptyReplacementsDictionary, false);

            // Assert
            VerifyWrite(_outWriter, "Extracting variables");
            VerifyWrite(_errorWriter, CliExecutionError.Message);

            _jsonTemplateValuesReplacerMock.VerifyNoOtherCalls();
            _apiConsumerMock.VerifyNoOtherCalls();
        }

        [Theory, IsUnit]
        [InlineData(true, 1)]
        [InlineData(false, 0)]
        public async Task ProcessAsync_NoDryRunValue_ControlsIfApiConsumerIsCalled(bool noDryRun, int timesApiConsumerIsCalled)
        {
            var environment = "CI";
            // Arrange
            _subscribersDirectoryProcessorMock.Setup(x => x.ProcessDirectory(It.IsAny<string>()))
                .Returns(new List<string> { "file1.json", "file2.json" });

            _subscriberFileParserMock.Setup(x => x.ParseFile("file1.json"))
                .Returns(JObject.FromObject(ValidSubscriberFileJson));

            _subscriberFileParserMock.Setup(x => x.ParseFile("file2.json"))
                .Returns(CliExecutionError);

            _jsonVarsExtractorMock.Setup(x => x.ExtractVars(It.IsAny<JObject>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, JToken>());

            _jsonTemplateValuesReplacerMock.Setup(x => x.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, JToken>>()))
                .Returns(ValidSubscriberRequest);

            _apiConsumerMock.Setup(x => x.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()))
                .Returns(EmptyAsyncEnumerable());

            // Act
            await _sut.ProcessAsync("a folder", environment, EmptyReplacementsDictionary, noDryRun);

            // Assert
            _apiConsumerMock.Verify(x => x.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()), Times.Exactly(timesApiConsumerIsCalled));
        }

        [Fact, IsUnit]
        public async Task ProcessAsync_ValidFiles_OutputMessages()
        {
            var environment = "CI";
            // Arrange
            _subscribersDirectoryProcessorMock.Setup(x => x.ProcessDirectory(It.IsAny<string>()))
                .Returns(new List<string> { "file1.json", "file2.json" });

            _subscriberFileParserMock.Setup(x => x.ParseFile("file1.json"))
                .Returns(JObject.FromObject(ValidSubscriberFileJson));

            _subscriberFileParserMock.Setup(x => x.ParseFile("file2.json"))
                .Returns(JObject.FromObject(ValidSubscriberFileJson));

            _jsonVarsExtractorMock.Setup(x => x.ExtractVars(It.IsAny<JObject>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, JToken>
                {
                    ["invoicing-url"] = "https://log1-randomapi.eshopworld.net/api/v2.0/MethodOne/ActionItem"
                });

            _jsonTemplateValuesReplacerMock.Setup(x => x.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, JToken>>()))
                .Returns(ValidSubscriberRequest);

            _apiConsumerMock.Setup(x => x.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()))
                .Returns(EmptyAsyncEnumerable());

            // Act
            var result = await _sut.ProcessAsync("a folder", environment, new Dictionary<string, string>()
            {
                ["subname"] = "subscriber1"
            }, false);

            // Assert
            result.Should().Be(0);

            // Verify Output messages
            VerifyWrite(_outWriter, "Reading files from folder: 'a folder' to be run against CI environment");
            VerifyWrite(_outWriter, "Processing file: '..\\file1.json'");
            VerifyWrite(_outWriter, "Processing file: '..\\file2.json'");
            VerifyWrite(_outWriter, "Extracting variables", Times.Exactly(2));
            VerifyWrite(_outWriter, "Replacing vars in template", Times.Exactly(2));
            VerifyWrite(_outWriter, "Replacing params in template", Times.Exactly(2));
            VerifyWrite(_outWriter, "File successfully parsed", Times.Exactly(2));
            VerifyWrite(_outWriter, "By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
        }

        private static void VerifyWrite(Mock<TextWriter> mockWriter, string text, Times times)
        {
            mockWriter.Verify(x => x.WriteLine(It.Is<string>(s => s.Contains(text))), times);
        }

        private static void VerifyWrite(Mock<TextWriter> mockWriter, string text)
        {
            VerifyWrite(mockWriter, text, Times.Once());
        }
    }
}
