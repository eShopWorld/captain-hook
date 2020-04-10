using CaptainHook.Cli.Tests.Utilities;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CaptainHook.Cli.Tests
{
    public abstract class CliTestBase
    {
        private readonly ITestOutputHelper output;
        protected readonly TestConsole Console;
        protected string Output => (output as TestOutputHelper)?.Output?.TrimEnd('\r', '\n');

        public CliTestBase(ITestOutputHelper output)
        {
            this.output = output;
            Console = new TestConsole(output);
        }
    }
}
