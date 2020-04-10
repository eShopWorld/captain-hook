using CaptainHook.Tests.Cli.Utilities;
using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CaptainHook.Tests.Cli
{
    public class VersionTest : CliTestBase
    {
        public VersionTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsLayer2]
        public void CheckVersionMatch()
        {
            CommandLineApplication app = new CommandLineApplication<CaptainHook.Cli.Program>(Console);
            app.Conventions.UseDefaultConventions();
            app.Execute("--version");
            Output.Should().Be(typeof(CaptainHook.Cli.Program).Assembly.GetName().Version.ToString(3));
        }
    }
}
