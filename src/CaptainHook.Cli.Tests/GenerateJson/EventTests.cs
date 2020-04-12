using CaptainHook.Cli.Commands.GenerateJson;
using Eshopworld.Tests.Core;
using FluentAssertions;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public class EventTests : GenerateJsonCommandBase
    {
        public EventTests(ITestOutputHelper output): base(output)
        {
        }

        [Fact, IsLayer0]
        public async Task GeneratesTwoEventFiles()
        {
            Prepare();
            await Command.OnExecuteAsync(Application, Console);
            FileSystem.Directory.GetFiles(OutputFolderPath).Count().Should().Be(2);
        }

        [Fact, IsLayer0]
        public async Task FileNamesAreCorrect()
        {
            Prepare();
            await Command.OnExecuteAsync(Application, Console);
            FileSystem.FileExists(@"C:\output\event-1-activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent.json").Should().BeTrue();
            FileSystem.FileExists(@"C:\output\event-2-activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent.json").Should().BeTrue();
        }
    }
}
