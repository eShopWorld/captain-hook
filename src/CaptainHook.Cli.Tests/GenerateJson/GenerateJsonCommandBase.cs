using CaptainHook.Cli.Commands.GenerateJson;
using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public abstract class GenerateJsonCommandBase : CliTestBase
    {
        protected readonly MockFileSystem FileSystem = new MockFileSystem();
        protected readonly string InputFilePath = @"C:\input\input.ps1";
        protected readonly string OutputFolderPath = @"C:\output";
        protected GenerateJsonCommand Command;

        public GenerateJsonCommandBase(ITestOutputHelper output) : base(output)
        {
            FileSystem.AddFile(InputFilePath, File.ReadAllText("SampleFile.ps1"));
        }

        protected JObject JsonResult => GetJsonResult();

        private JObject GetJsonResult()
        {
            var filename = @"event-1-activity1.domain.infrastructure.domainevents.activityconfirmationdomainevent.json";
            var path = Path.Combine(OutputFolderPath, filename);
            var fileContent = FileSystem.GetFile(path);
            return JObject.Parse(fileContent.TextContents);
        }

        protected void PrepareCommand()
        {
            Command = new GenerateJsonCommand(FileSystem)
            {
                InputFilePath = InputFilePath,
                OutputFolderPath = OutputFolderPath
            };
        }

        protected void PrepareCommand(string inputFilePath, string outputFolderPath)
        {
            Command = new GenerateJsonCommand(FileSystem)
            {
                InputFilePath = inputFilePath,
                OutputFolderPath = outputFolderPath
            };
        }
    }
}
