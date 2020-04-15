using CaptainHook.Cli.Tests.Utilities;
using McMaster.Extensions.CommandLineUtils;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CaptainHook.Cli.Tests
{
    public abstract class CliTestBase
    {
        private ITestOutputHelper output;
        protected readonly IConsole Console;
        protected string Output => (output as TestOutputHelper)?.Output?.TrimEnd('\r', '\n');
        protected readonly CommandLineApplication Application;

        protected CliTestBase(ITestOutputHelper output)
        {
            this.output = output;
            Console = new TestConsole(output);
            Application = new CommandLineApplication<Program>(Console);
        }
    }
}
