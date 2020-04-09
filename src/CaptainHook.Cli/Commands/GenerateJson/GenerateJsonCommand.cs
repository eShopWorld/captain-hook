using CaptainHook.Cli.Extensions;
using CaptainHook.Cli.Services;
using CaptainHook.Cli.Telemetry;
using Eshopworld.Core;
using McMaster.Extensions.CommandLineUtils;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    /// <summary>
    /// A command to generate a set of JSON files from a Captain Hook setup powershell script
    /// </summary>
    [Command("generateJson", Description = "generates JSON files from a Captain Hook setup powershell script"), HelpOption]
    public class GenerateJsonCommand: GenerateJsonBase
    {
        private readonly PathService pathService;
        private JsonSerializerOptions serializerOptions = new JsonSerializerOptions
        {
            IgnoreNullValues = true
        };

        public GenerateJsonCommand(PathService pathService)
        {
            this.pathService = pathService;
        }

        /// <summary>
        /// The path to the powershell script file (.ps1). Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the powershell script file (.ps1). Can be absolute or relative.",
            ShortName = "i",
            LongName = "input",
            ShowInHelpText = true)]
        [Required]
        public string InputFilePath { get; set; }

        /// <summary>
        /// The path to the output folder that will contain the JSON files. Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the output folder that will contain the JSON files. Can be absolute or relative.",
            ShortName = "o",
            LongName = "output",
            ShowInHelpText = true)]

        public string OutputFolderPath { get; set; }

        /// <summary>
        /// Command execution
        /// </summary>
        /// <param name="app">The application</param>
        /// <param name="console">Console reference</param>
        /// <returns>Return code</returns>
        public Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var sourceFile = Path.GetFullPath(InputFilePath);

            string outputFolder = string.Empty;
            if (string.IsNullOrWhiteSpace(OutputFolderPath))
            {
                outputFolder = Path.GetDirectoryName(GetType().Assembly.Location);
            }
            else
            {
                outputFolder = Path.GetFullPath(OutputFolderPath);
            }

            if(!File.Exists(sourceFile))
            {
                console.EmitWarning(GetType(), app.Options, $"Cannot open {InputFilePath}");

                return Task.FromResult(1);
            }

            pathService.CreateDirectory(outputFolder);

            var inputFileContent = ReadText(sourceFile);

            var models = ConvertToModels(inputFileContent);

            string jsonString = string.Empty;
            string filename = string.Empty;

            foreach(var (i, model) in models)
            {
                jsonString = JsonSerializer.Serialize(model, serializerOptions);
                
                filename = $"event-{i}-{model.Name}.json";

                File.WriteAllText(Path.Combine(outputFolder, filename), jsonString);
            }

            return Task.FromResult(0);
        }
    }
}
