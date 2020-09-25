using Newtonsoft.Json.Linq;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using Platform.Eda.Cli.Commands.GenerateJson;
using Xunit.Abstractions;

namespace CaptainHook.Cli.Tests.GenerateJson
{
    public abstract class GenerateJsonCommandBase : CliTestBase
    {
        protected readonly MockFileSystem FileSystem = new MockFileSystem();
        protected const string InputFilePath = @"C:\input\input.ps1";
        protected const string OutputFolderPath = @"C:\output";
        protected GenerateJsonCommand Command;

        protected GenerateJsonCommandBase(ITestOutputHelper output) : base(output)
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

        protected void PrepareCommand(string inputFilePath = InputFilePath, string outputFolderPath = OutputFolderPath)
        {
            Command = new GenerateJsonCommand(FileSystem)
            {
                InputFilePath = inputFilePath,
                OutputFolderPath = outputFolderPath
            };
        }
    }
}
