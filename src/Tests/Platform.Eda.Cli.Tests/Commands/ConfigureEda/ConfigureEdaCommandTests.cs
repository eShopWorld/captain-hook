using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using CaptainHook.Cli.Tests;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Platform.Eda.Cli.Commands.ConfigureEda;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Xunit;
using Xunit.Abstractions;

namespace Platform.Eda.Cli.Tests.Commands.ConfigureEda
{
    public class ConfigureEdaCommandTests : CliTestBase
    {
        internal const string MockCurrentDirectory = @"Z:\Sample\";
        private readonly ConfigureEdaCommand _configureEdaCommand;
        private readonly Mock<IPutSubscriberProcessChain> _mockPutSubscriberProcessChain;
        private readonly ConsoleSubscriberWriter _consoleSubscriberWriter;


        public ConfigureEdaCommandTests(ITestOutputHelper output) : base(output)
        {
            _mockPutSubscriberProcessChain = new Mock<IPutSubscriberProcessChain>();
            _consoleSubscriberWriter = new ConsoleSubscriberWriter(Console);
            _configureEdaCommand = new ConfigureEdaCommand(_mockPutSubscriberProcessChain.Object)
            {
                InputFolderPath = MockCurrentDirectory,
                NoDryRun = true,
                Environment = "CI",
                Params = new [] { "abc=def" }
            };
        }

        [Fact, IsUnit]
        public async Task OnExecuteAsync_WhenProcessSuccessful_Returns0()
        {
            _mockPutSubscriberProcessChain.Setup(processor =>
                    processor.ProcessAsync(_configureEdaCommand.InputFolderPath, _configureEdaCommand.Environment,
                        It.IsAny<Dictionary<string, string>>(), _configureEdaCommand.NoDryRun))
                .ReturnsAsync(0);

            var result = await _configureEdaCommand.OnExecuteAsync(_consoleSubscriberWriter);
            
            result.Should().Be(0);
            Output.SplitLines().Should().Contain("Processing finished");
        }


        public static IEnumerable<object[]> ParamsTestData = new List<object[]>
        {
            new object[] { null, null },
            new object[] { new string[0], new Dictionary<string, string>()  }
        };

        [Theory, IsUnit]
        [MemberData(nameof(ParamsTestData))]
        public async Task OnExecuteAsync_WhenNoParams_EmptyDictionaryIsPassedToProcessChain(string[] param, Dictionary<string, string> paramsDictionary)
        {
            _mockPutSubscriberProcessChain.Setup(processor =>
                    processor.ProcessAsync(_configureEdaCommand.InputFolderPath, _configureEdaCommand.Environment,
                        paramsDictionary, _configureEdaCommand.NoDryRun))
                .ReturnsAsync(0);
            _configureEdaCommand.Params = param;

            var result = await _configureEdaCommand.OnExecuteAsync(_consoleSubscriberWriter);

            result.Should().Be(0);
            Output.SplitLines().Should().Contain("Processing finished");
        }
    }
}