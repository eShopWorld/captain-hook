using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.Linq;
using CaptainHook.Cli.Commands.GenerateJson;
using CaptainHook.Cli.Commands.GeneratePowerShell;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace CaptainHook.Cli.Tests.GeneratePowerShell
{
    public class ConfigDirectoryProcessorTests
    {
        private readonly MockFileSystem fileSystem = new MockFileSystem();

        public ConfigDirectoryProcessorTests()
        {
            fileSystem.AddFile("C:\\Test\\event-1-testevent.json", $"{{\r\n\"Type\": \"type1\" \r\n}}");
            fileSystem.AddFile("C:\\Test\\event-10-testevent.json", $"{{\r\n\"Type\": \"type10\" \r\n}}");
            fileSystem.AddFile("C:\\Test\\event-11-testevent.json", $"{{\r\n\"Type\": \"type11\" \r\n}}");
            fileSystem.AddFile("C:\\Test\\event-101-testevent.json", $"{{\r\n\"Type\": \"type101\" \r\n}}");
            fileSystem.AddFile("C:\\Test\\event-2-testevent.json", $"{{\r\n\"Type\": \"type2\" \r\n}}");

        }

        [Fact, IsLayer0]
        public void ShouldProcessFilesInCorrectOrder()
        {
            var expected = new[]
            {
                "setConfig 'event--1--type' 'type1' $KeyVault",
                "setConfig 'event--2--type' 'type2' $KeyVault",
                "setConfig 'event--10--type' 'type10' $KeyVault",
                "setConfig 'event--11--type' 'type11' $KeyVault",
                "setConfig 'event--101--type' 'type101' $KeyVault",
            };

            var result = new ConfigDirectoryProcessor(fileSystem).ProcessDirectory("C:\\Test\\", "result.ps1");
            result.Success.Should().BeTrue();

            var actual = fileSystem.File.ReadAllLines("result.ps1");
            actual.Should().Equal(expected);
        }
    }
}
