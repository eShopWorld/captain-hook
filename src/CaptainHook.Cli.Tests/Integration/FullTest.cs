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
        const string sourceFile = "SampleFileWithSubscribers.ps1";
        const string jsonDirectory = "json";
        const string resultFile = "Result.ps1";

        private readonly ITestOutputHelper outputHelper;

        public FromPowerShellToJsonAndBackTests(ITestOutputHelper outputHelper)
        {
            this.outputHelper = outputHelper;
        }

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

        private void RunCaptainHookCli(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "CaptainHook.Cli.exe",
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
