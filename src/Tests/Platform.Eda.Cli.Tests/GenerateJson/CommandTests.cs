using Eshopworld.Tests.Core;
using FluentAssertions;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public class CommandTests : GenerateJsonCommandBase
    {
        public CommandTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact, IsUnit]
        public async Task NullInputFilePathThrowsError()
        {
            PrepareCommand(null, null);
            Task result() => Command.OnExecuteAsync(Application, Console);
            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact, IsUnit]
        public async Task EmptyInputFilePathThrowsError()
        {
            PrepareCommand(string.Empty, null);
            Task result() => Command.OnExecuteAsync(Application, Console);
            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact, IsUnit]
        public async Task FileNotFoundReturnsError()
        {
            PrepareCommand("NonExistentFile", null);
            var result = await Command.OnExecuteAsync(Application, Console);
            result.Should().Be(1);
        }

        [Fact, IsUnit]
        public async Task GeneratesTwoEventFiles()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            FileSystem.Directory.GetFiles(OutputFolderPath).Count().Should().Be(2);
        }

        [Fact, IsUnit]
        public async Task FileNamesAreCorrect()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            FileSystem.FileExists(@"C:\output\event-1-activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent.json").Should().BeTrue();
            FileSystem.FileExists(@"C:\output\event-2-activity1.domain.infrastructure.domainevents.platformactivitycreatedomainevent.json").Should().BeTrue();
        }

        [Fact, IsUnit]
        public async Task FileContainsJson()
        {
            PrepareCommand();
            await Command.OnExecuteAsync(Application, Console);
            var exception = Record.Exception(() => JsonResult);
            Assert.Null(exception);
        }
    }
}
