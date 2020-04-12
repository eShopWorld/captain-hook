using CaptainHook.Cli.Commands.GenerateJson;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public abstract class GenerateJsonCommandBase: CliTestBase
    {
        protected readonly MockFileSystem FileSystem = new MockFileSystem();
        protected readonly string InputFilePath = @"C:\input\input.ps1";
        protected readonly string OutputFolderPath = @"C:\output";
        protected GenerateJsonCommand Command;

        public GenerateJsonCommandBase(ITestOutputHelper output): base(output)
        {
            FileSystem.AddFile(InputFilePath, File.ReadAllText("SampleFile.ps1"));
        }
        protected void Prepare()
        {
            Command = new GenerateJsonCommand(FileSystem)
            {
                InputFilePath = InputFilePath,
                OutputFolderPath = OutputFolderPath
            };
        }
    }
}
