using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
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
        private static readonly CliExecutionError CliExecutionError = new CliExecutionError(string.Empty);
        private static readonly Dictionary<string, string> EmptyReplacementsDictionary = new Dictionary<string, string>();

        private readonly Mock<IConsoleSubscriberWriter> _consoleSubscriberWriterMock;
        private readonly Mock<ISubscribersDirectoryProcessor> _subscribersDirectoryProcessorMock;
        private readonly Mock<ISubscriberFileParser> _subscriberFileParserMock;
        private readonly Mock<IJsonVarsExtractor> _jsonVarsExtractorMock;
        private readonly Mock<IJsonTemplateValuesReplacer> _jsonTemplateValuesReplacerMock;
        private readonly Mock<IApiConsumer> _apiConsumerMock;

        private static readonly string _validSubscriberRequestContent = @"
              ""eventName"": ""eshopworld.platform.events.oms.lineitemcancelsucceededevent"",
              ""subscriberName"": ""invoicing"",
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

        private static readonly string _validSubscriberRequest = $"{{{_validSubscriberRequestContent}}}";

        private static readonly JObject _validSubscriberFileJson = JObject.Parse(@$"
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

        private readonly PutSubscriberProcessChain _sut;

        public PutSubscriberProcessChainTests()
        {
            _consoleSubscriberWriterMock = new Mock<IConsoleSubscriberWriter>();
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
                _consoleSubscriberWriterMock.Object,
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
            using(new AssertionScope())
            _subscribersDirectoryProcessorMock.Verify(x => x.ProcessDirectory(invalidFolder), Times.Once);
            _consoleSubscriberWriterMock.Verify(x => x.WriteError(CliExecutionError.Message), Times.Once);
            result.Should().Be(1);
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
                .Returns(_validSubscriberFileJson);
            _subscriberFileParserMock
                .Setup(x => x.ParseFile("file2.json"))
                .Returns(CliExecutionError);
            _jsonVarsExtractorMock
                .Setup(x => x.ExtractVars(It.IsAny<JObject>(), It.IsAny<string>()))
                .Returns(new Dictionary<string, JToken>());
            _jsonTemplateValuesReplacerMock
                .Setup(x => x.Replace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, JToken>>()))
                .Returns(_validSubscriberRequest);

            async IAsyncEnumerable<ApiOperationResult> EmptyAsyncEnumerable()
            {
                yield return new ApiOperationResult
                {
                    File = new FileInfo("file1.json"),
                    Request = new PutSubscriberRequest(),
                    Response = new HttpOperationResponse()
                };
                await Task.CompletedTask;
            }

            _apiConsumerMock
                .Setup(x => x.CallApiAsync(It.IsAny<IEnumerable<PutSubscriberFile>>()))
                .Returns(EmptyAsyncEnumerable());

            // Act
            var result = await _sut.ProcessAsync("a folder", environment, EmptyReplacementsDictionary, true);

            // Assert
            using (new AssertionScope())
            _subscriberFileParserMock.Verify(x => x.ParseFile("file1.json"), Times.Once);
            _subscriberFileParserMock.Verify(x => x.ParseFile("file2.json"), Times.Once);
            _consoleSubscriberWriterMock.Verify(x => x.WriteError(CliExecutionError.Message), Times.Once);
            _consoleSubscriberWriterMock.Verify(x => x.OutputSubscribers(It.IsAny<IEnumerable<PutSubscriberFile>>(), "a folder"), Times.Once);
            result.Should().Be(0);
        }

    }
}
