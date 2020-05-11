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
        [Fact, IsLayer1]
        public void OutputFileShouldBeSameAsInputFile()
        {
            var toolDirectory = Directory.GetCurrentDirectory();

            const string toolName = "CaptainHook.Cli.exe";
            const string sourceFile = "SampleFileWithSubscribers.ps1";
            const string jsonDirectory = "json";
            const string resultFile = "Result.ps1";

            string generateJsonCommand = $@"generate-json --input ""{sourceFile}"" --output ""{jsonDirectory}""";

            var generateJsonProcess = Process.Start(toolName, generateJsonCommand);
            generateJsonProcess.WaitForExit();

            File.Copy("Header.ps1", $@"{jsonDirectory}\Header.ps1", true);
            
            string generatePowerShellCommand = $@"generate-powershell --input ""{jsonDirectory}"" --output ""{resultFile}""";

            var generatePowerShellProcess = Process.Start(toolName, generatePowerShellCommand);
            generatePowerShellProcess.WaitForExit();

            generateJsonProcess.ExitCode.Should().Be(0);
            generatePowerShellProcess.ExitCode.Should().Be(0);

            var original = CleanPowershell(File.ReadAllLines(sourceFile)).ToArray();
            var result = CleanPowershell(File.ReadAllLines(resultFile)).ToArray();

            result.Should().BeEquivalentTo(original);
        }

        private IEnumerable<string> CleanPowershell(IEnumerable<string> input)
        {
            return input.Where(s => s.TrimStart().StartsWith("set"));
        }
    }
}
