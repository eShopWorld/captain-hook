using System.IO.Abstractions.TestingHelpers;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Platform.Eda.Cli.Commands.GeneratePowerShell;
using Xunit;

namespace CaptainHook.Cli.Tests.GeneratePowerShell
{
    public class ConfigDirectoryProcessorTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();

        public ConfigDirectoryProcessorTests()
        {
            fileSystem.AddFile(@"C:\Test\event-1-testevent.json", $"{{\r\n\"Type\": \"type1\" \r\n}}");
            fileSystem.AddFile(@"C:\Test\event-10-testevent.json", $"{{\r\n\"Type\": \"type10\" \r\n}}");
            fileSystem.AddFile(@"C:\Test\event-11-testevent.json", $"{{\r\n\"Type\": \"type11\" \r\n}}");
            fileSystem.AddFile(@"C:\Test\event-101-testevent.json", $"{{\r\n\"Type\": \"type101\" \r\n}}");
            fileSystem.AddFile(@"C:\Test\event-2-testevent.json", $"{{\r\n\"Type\": \"type2\",\r\n\"HeartBeatInterval\":\"\" \r\n}}");
            fileSystem.AddFile(@"C:\Test\event-3-testevent.json", $"{{\r\n\"Type\": \"type3\",\r\n\"HeartBeatInterval\":\"00:00:05\" \r\n}}");

        }

        [Fact, IsUnit]
        public void ShouldProcessFilesInCorrectOrder()
        {
            var expected = new[]
            {
                "setConfig 'event--1--type' 'type1' $KeyVault",
                "setConfig 'event--2--type' 'type2' $KeyVault",
                "setConfig 'event--2--heartbeatinterval' '' $KeyVault",
                "setConfig 'event--3--type' 'type3' $KeyVault",
                "setConfig 'event--3--heartbeatinterval' '00:00:05' $KeyVault",
                "setConfig 'event--10--type' 'type10' $KeyVault",
                "setConfig 'event--11--type' 'type11' $KeyVault",
                "setConfig 'event--101--type' 'type101' $KeyVault",
            };

            var result = new ConfigDirectoryProcessor(fileSystem).ProcessDirectory("C:\\Test\\", "result.ps1");
            result.Success.Should().BeTrue();

            var actual = fileSystem.File.ReadAllLines("result.ps1");
            actual.Should().Equal(expected);
        }

        [Fact, IsUnit]
        public void ShouldProcessHeaderAndIncludeInOutput()
        {
            fileSystem.AddFile(@"C:\Test\header.ps1", "abcde\r\ndef from header");
            var configDirectoryProcessor = new ConfigDirectoryProcessor(fileSystem);

            var result = configDirectoryProcessor.ProcessDirectory(@"C:\Test\", "result.ps1");

            var expectedOutput = new[]
            {
                "abcde",
                "def from header",
                "setConfig 'event--1--type' 'type1' $KeyVault",
                "setConfig 'event--2--type' 'type2' $KeyVault",
                "setConfig 'event--2--heartbeatinterval' '' $KeyVault",
                "setConfig 'event--3--type' 'type3' $KeyVault",
                "setConfig 'event--3--heartbeatinterval' '00:00:05' $KeyVault",
                "setConfig 'event--10--type' 'type10' $KeyVault",
                "setConfig 'event--11--type' 'type11' $KeyVault",
                "setConfig 'event--101--type' 'type101' $KeyVault",
            };

            using (new AssertionScope())
            {
                result.Success.Should().BeTrue();

                var actualOutput = fileSystem.File.ReadAllLines("result.ps1");
                actualOutput.Should().Equal(expectedOutput);
            }
        }
    }
}
