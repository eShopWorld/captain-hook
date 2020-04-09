using CaptainHook.Cli;
using Eshopworld.Tests.Core;
using FluentAssertions;
using System;
using Xunit;

namespace CaptainHook.Tests.Cli
{
    public class VersionTest : CliTestBase
    {
        [Fact, IsLayer2]
        public void CheckVersionMatch()
        {
            var console = GetStandardOutput("--version");
            // check we are getting the expected version of CLI when invoking the command
            console.TrimEnd().Should().Be(typeof(Program).Assembly.GetName().Version.ToString(3));
        }
    }
}
