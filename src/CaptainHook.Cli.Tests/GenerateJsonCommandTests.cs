using CaptainHook.Cli.Commands.GenerateJson;
using CaptainHook.Cli.Tests.Utilities;
using Eshopworld.Tests.Core;
using FluentAssertions;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json.Linq;
using System;
using System.IO.Abstractions.TestingHelpers;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests
{
    //public class GenerateJsonCommandTests : CliTestBase
    //{
    //    private readonly CommandLineApplication application;
    //    private readonly MockFileSystem fileSystem = new MockFileSystem();
    //    private const string aFilePath = @"c:\input.ps1";

    //    private JObject Output() => JObject.Parse(fileSystem.GetFile(aFilePath).TextContents);
 
    //    public GenerateJsonCommandTests(ITestOutputHelper output) : base(output)
    //    {
    //        application = new CommandLineApplication<Program>(Console);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task NullInputFilePathThrowsError()
    //    {
    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = null;

    //        Task result() => command.OnExecuteAsync(application, Console);

    //        await Assert.ThrowsAsync<ArgumentNullException>(result);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task EmptyInputFilePathThrowsError()
    //    {
    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = string.Empty;

    //        Task result() => command.OnExecuteAsync(application, Console);

    //        await Assert.ThrowsAsync<ArgumentException>(result);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task FileNotFoundReturnsError()
    //    {
    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = @"NonExistentFile";

    //        var result = await command.OnExecuteAsync(application, Console);

    //        result.Should().Be(1);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task ValidFileNoHeaderContent()
    //    {
    //        fileSystem.AddFile(aFilePath, Resources.ValidFileNoHeaderContent);

    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = aFilePath;

    //        var result = await command.OnExecuteAsync(application, Console);

    //        result.Should().Be(0);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task SingleRouteFileContent()
    //    {
    //        fileSystem.AddFile(aFilePath, Resources.SingleRouteFileContent);

    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = aFilePath;

    //        var result = await command.OnExecuteAsync(application, Console);

    //        result.Should().Be(0);
    //    }

    //    [Fact, IsLayer0]
    //    public async Task AuthFileContent()
    //    {
    //        fileSystem.AddFile(aFilePath, Resources.AuthFileContent);

    //        var command = new GenerateJsonCommand(fileSystem);
    //        command.InputFilePath = aFilePath;

    //        var result = await command.OnExecuteAsync(application, Console);

    //        result.Should().Be(0);
    //    }

    //}
}
