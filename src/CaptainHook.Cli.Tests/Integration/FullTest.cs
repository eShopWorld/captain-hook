using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Cli.Tests.Integration
{
    public class FromPowerShellToJsonAndBackTests
    {
        const string sourceFile = "SampleFileWithSubscribers.ps1";
        const string jsonDirectory = "json";
        const string resultFile = "Result.ps1";

        [Fact, IsLayer1]
        public void OutputFileShouldBeSameAsInputFile()
        {
            RunCaptainHookCli($@"generate-json --input ""{sourceFile}"" --output ""{jsonDirectory}""");
            File.Copy("Header.ps1", $@"{jsonDirectory}\Header.ps1", true);
            RunCaptainHookCli($@"generate-powershell --input ""{jsonDirectory}"" --output ""{resultFile}""");

            var original = File.ReadAllLines(sourceFile).TakeOnlyCommands();
            var result = File.ReadAllLines(resultFile).TakeOnlyCommands();

            result.Should().BeEquivalentTo(original);
        }

        private static void RunCaptainHookCli(string formattableString)
        {
            var generateJsonProcess = Process.Start("CaptainHook.Cli.exe", formattableString);
            generateJsonProcess.WaitForExit();
            generateJsonProcess.ExitCode.Should().Be(0);
        }
    }

    internal static class TestPowerShellStringExtensions
    {
        public static IEnumerable<string> TakeOnlyCommands(this IEnumerable<string> input)
        {
            return input.Where(s => s.TrimStart().StartsWith("set"));
        }
    }
}
