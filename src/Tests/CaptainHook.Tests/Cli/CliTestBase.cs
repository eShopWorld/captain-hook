using FluentAssertions;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace CaptainHook.Tests.Cli
{
    public abstract class CliTestBase
    {
        private readonly int processTimeout = (int)TimeSpan.FromMinutes(5).TotalMilliseconds;

        // ReSharper disable once InconsistentNaming
        private Process RunCli(bool redirect = false, params string[] parameters)
        {
            var p = new Process();
            var sb = new StringBuilder();

            sb.AppendJoin(' ', "/c", "dotnet", "ch "); //the space is important here at the end

            if (parameters.Length > 0)
                sb.AppendJoin(' ', parameters);

            p.StartInfo = new ProcessStartInfo("cmd.exe", sb.ToString()) { CreateNoWindow = false, RedirectStandardOutput = redirect, RedirectStandardError = redirect };
            p.Start().Should().BeTrue();
            p.WaitForExit((int)processTimeout).Should().BeTrue(); //never mind the cast

            return p;
        }

        protected string GetErrorOutput(params string[] parameters)
        {
            using (var p = RunCli(true, parameters))
            {
                var errorStream = p.StandardError.ReadToEnd();
                errorStream.Should().NotBeNullOrEmpty();
                p.ExitCode.Should().NotBe(0);
                return errorStream;
            }
        }

        protected string GetStandardOutput(params string[] parameters)
        {
            using (var p = RunCli(true, parameters))
            {
                p.StandardError.ReadToEnd().Should().BeNullOrWhiteSpace();
                p.ExitCode.Should().Be(0);
                return p.StandardOutput.ReadToEnd();
            }
        }

        protected void InvokeCLI(params string[] parameters)
        {
            using (var p = RunCli(false, parameters))
            {
                p.ExitCode.Should().Be(0);
            }
        }

        protected static void DeleteTestFiles(string basePath, params string[] fileNames)
        {
            foreach (var f in fileNames)
            {
                File.Delete(Path.Combine(basePath, f));
            }
        }
    }
}
