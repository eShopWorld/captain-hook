using AutoFixture;
using CaptainHook.Cli.Commands.GenerateJson;
using CaptainHook.Cli.Tests.Utilities;
using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Moq;
using System;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests
{
    public class GenerateJsonCommandTests : CliTestBase
    {
        private readonly CommandLineApplication application;
        private readonly Mock<IFileSystem> fileSystem;

        public GenerateJsonCommandTests(ITestOutputHelper output) : base(output)
        {
            fileSystem = new Mock<IFileSystem>();
            
            fileSystem.Setup(f => f.Directory.CreateDirectory(It.IsAny<String>()));

            fileSystem.Setup(f => f.File.WriteAllText(It.IsAny<String>(), It.IsAny<String>())).Verifiable();
            fileSystem.Setup(f => f.File.Exists(It.Is<string>(x => x.EndsWith("NonExistentFile")))).Returns(false);

            fileSystem.Setup(f => f.File.Exists(It.Is<string>(x => x.EndsWith("ValidFileNoHeaderContent")))).Returns(true);
            fileSystem.Setup(f => f.File.ReadAllText(It.Is<string>(x => x.EndsWith("ValidFileNoHeaderContent")))).Returns(Resources.ValidFileNoHeaderContent);

            fileSystem.Setup(f => f.File.Exists(It.Is<string>(x => x.EndsWith("SingleRouteFileContent")))).Returns(true);
            fileSystem.Setup(f => f.File.ReadAllText(It.Is<string>(x => x.EndsWith("SingleRouteFileContent")))).Returns(Resources.SingleRouteFileContent);

            fileSystem.Setup(f => f.File.Exists(It.Is<string>(x => x.EndsWith("AuthFileContent")))).Returns(true);
            fileSystem.Setup(f => f.File.ReadAllText(It.Is<string>(x => x.EndsWith("AuthFileContent")))).Returns(Resources.AuthFileContent);
            
            application = new CommandLineApplication<Program>(Console);
        }

        [Fact, IsLayer0]
        public async Task NullInputFilePathThrowsError()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = null;

            Task result() => command.OnExecuteAsync(application, Console);

            await Assert.ThrowsAsync<ArgumentNullException>(result);
        }

        [Fact, IsLayer0]
        public async Task EmptyInputFilePathThrowsError()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = string.Empty;

            Task result() => command.OnExecuteAsync(application, Console);

            await Assert.ThrowsAsync<ArgumentException>(result);
        }

        [Fact, IsLayer0]
        public async Task FileNotFoundReturnsError()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = @"NonExistentFile";

            var result = await command.OnExecuteAsync(application, Console);

            result.Should().Be(1);
        }

        [Fact, IsLayer0]
        public async Task ValidFileNoHeaderContent()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = @"ValidFileNoHeaderContent";

            var result = await command.OnExecuteAsync(application, Console);

            result.Should().Be(0);
        }

        [Fact, IsLayer0]
        public async Task SingleRouteFileContent()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = @"SingleRouteFileContent";

            var result = await command.OnExecuteAsync(application, Console);

            result.Should().Be(0);
        }

        [Fact, IsLayer0]
        public async Task AuthFileContent()
        {
            var command = new GenerateJsonCommand(fileSystem.Object);
            command.InputFilePath = @"AuthFileContent";

            var result = await command.OnExecuteAsync(application, Console);

            result.Should().Be(0);
        }

    }
}
