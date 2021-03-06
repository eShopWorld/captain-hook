using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests
{
    public class VersionTest : CliTestBase
    {
        public VersionTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsUnit]
        public void CheckVersionMatch()
        {
            CommandLineApplication app = new CommandLineApplication<Program>(Console);
            app.Conventions.UseDefaultConventions();
            app.Execute("--version");
            Output.Should().Be(typeof(Program).Assembly.GetName().Version.ToString(3));
        }
    }
}
