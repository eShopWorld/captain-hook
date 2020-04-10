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
    public class VersionTest
    {
        private readonly ITestOutputHelper output;

        public VersionTest(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact, IsLayer2]
        public void CheckVersionMatch()
        {
            CommandLineApplication app = new CommandLineApplication<CaptainHook.Cli.Program>(new TestConsole(output));
            app.Conventions.UseDefaultConventions();
            app.Execute("--version");
            (output as TestOutputHelper).Output.TrimEnd('\r','\n')
                .Should().Be(typeof(CaptainHook.Cli.Program).Assembly.GetName().Version.ToString(3));
        }
    }
}
