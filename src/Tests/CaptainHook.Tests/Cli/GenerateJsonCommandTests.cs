using CaptainHook.Cli.Commands.GenerateJson;
using Eshopworld.Tests.Core;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Tests.Cli
{
    public class GenerateJsonCommandTests: CliTestBase
    {
        public GenerateJsonCommandTests(ITestOutputHelper output):base(output)
        {
        }

        [Fact, IsLayer0]
        public void InvokeEmptyCommand()
        {
            var command = new GenerateJsonCommand(new CaptainHook.Cli.Services.PathService());
            command.
        }


    }
}
