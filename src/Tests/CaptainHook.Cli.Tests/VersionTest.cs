using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
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
            CommandLineApplication app = new CommandLineApplication<CaptainHook.Cli.Program>(Console);
            app.Conventions.UseDefaultConventions();
            app.Execute("--version");
            Output.Should().Be(typeof(CaptainHook.Cli.Program).Assembly.GetName().Version.ToString(3));
        }
    }
}
