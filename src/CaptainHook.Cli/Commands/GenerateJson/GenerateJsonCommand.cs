using CaptainHook.Cli.Extensions;
using CaptainHook.Cli.Services;
using CaptainHook.Cli.Telemetry;
using Eshopworld.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    /// <summary>
    /// A command to generate a set of JSON files from a Captain Hook setup powershell script
    /// </summary>
    [Command("generateJson", Description = "generates JSON files from a Captain Hook setup powershell script"), HelpOption]
    public class GenerateJsonCommand: GenerateJsonBase
    {
        private readonly IBigBrother bigBrother;
        private readonly PathService pathService;

        public GenerateJsonCommand(IBigBrother bigBrother, PathService pathService)
        {
            this.bigBrother = bigBrother;
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
        [Required]
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
            var outputFolder = Path.GetFullPath(OutputFolderPath);

            if(!File.Exists(sourceFile))
            {
                console.EmitWarning(GetType(), app.Options, $"Cannot open {InputFilePath}");

                return Task.FromResult(1);
            }

            pathService.CreateDirectory(outputFolder);

            var inputFileContent = ReadText(sourceFile);

            var json = ConvertToJson(inputFileContent);

            File.WriteAllText(outputFolder, json);

            bigBrother.Publish(new CaptainHookGeneratedJsonEvent { InputFile = sourceFile });

            return Task.FromResult(0);
        }
    }
}
