using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.Integration
{
    public class FromPowerShellToJsonAndBackTests
    {
        const string cliExe = "CaptainHook.Cli.exe";
        const string sourceFile = "SampleFileWithSubscribers.ps1";

        private readonly ITestOutputHelper outputHelper;

        public FromPowerShellToJsonAndBackTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

        [Fact(Skip = "Doesn't work in ADO, only to manual run"), IsUnit]
        public void OutputFileShouldBeSameAsInputFile()
        {
            string tempPath = Path.GetTempPath();
            string jsonDirectory = $@"{tempPath}\json";
            string resultFile =  $@"{tempPath}\Result.ps1";

            this.outputHelper.WriteLine($"Temp path: {tempPath}");

            RunProcess(cliExe, $@"generate-json --input ""{sourceFile}"" --output ""{jsonDirectory}""");
            File.Copy("Header.ps1", $@"{jsonDirectory}\Header.ps1", true);
            RunProcess(cliExe, $@"generate-powershell --input ""{jsonDirectory}"" --output ""{resultFile}""");

            var original = File.ReadAllLines(sourceFile).TakeOnlyCommands();
            var result = File.ReadAllLines(resultFile).TakeOnlyCommands();

            result.Should().BeEquivalentTo(original);
        }

        private void RunProcess(string executableFile, string arguments = null)
        {
            this.outputHelper.WriteLine($"Starting {executableFile} with arguments: `{arguments}`");
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = executableFile,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
            };
            process.ErrorDataReceived += OnDataReceived;

            process.Start();
            process.BeginErrorReadLine();

            string processOutput = process.StandardOutput.ReadToEnd();
            this.outputHelper.WriteLine(processOutput);

            process.ExitCode.Should().Be(0);
            this.outputHelper.WriteLine($"{executableFile} done");
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                this.outputHelper.WriteLine(e.Data);
            }
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
