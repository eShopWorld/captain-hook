using CaptainHook.Cli.Extensions;
using CaptainHook.Cli.Services;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using CaptainHook.Cli.ConfigurationProvider;
using System.Collections.Generic;
using CaptainHook.Common.Configuration;
using CaptainHook.Cli.Common;
using Newtonsoft.Json;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    /// <summary>
    /// A command to generate a set of JSON files from a Captain Hook setup powershell script
    /// </summary>
    [Command("generateJson", Description = "generates JSON files from a Captain Hook setup powershell script"), HelpOption]
    public class GenerateJsonCommand
    {
        private readonly PathService pathService;
        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = ShouldSerializeContractResolver.Instance,
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

            string outputFolder;

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

            var result = new ConfigurationBuilder()
                .AddEswPsFile(sourceFile)
                .Build()
                .GetSection("event")
                .Get<IEnumerable<EventHandlerConfig>>();

            foreach(var (@event, index) in result.WithIndex())
            {
                var jsonString = JsonConvert.SerializeObject(@event, jsonSettings);
                var filename = $"event-{1+index}-{@event.Name}.json";
                File.WriteAllText(Path.Combine(outputFolder, filename), jsonString);
            }

            return Task.FromResult(0);
        }
    }
}
