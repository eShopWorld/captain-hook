using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Cli.Common;
using CaptainHook.Cli.Extensions;
using CaptainHook.Common.Configuration;
using McMaster.Extensions.CommandLineUtils;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    [Command("generate-powershell", Description = "generates Captain Hook setup PowerShell script from  JSON files"), HelpOption]
    public class GeneratePowerShellCommand
    {
        private readonly IFileSystem fileSystem;

        private static readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new ShouldSerializeContractResolver(),
            Converters = new List<JsonConverter> { new StringEnumConverter() }
        };

        public GeneratePowerShellCommand(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <summary>
        /// The path to the input folder containing JSON files. Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the powershell script file (.ps1). Can be absolute or relative.",
            ShortName = "i",
            LongName = "input",
            ShowInHelpText = true)]
        [Required]
        public string InputFolderPath { get; set; }

        /// <summary>
        /// The path to the output PowerShell script file. Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the output folder that will contain the JSON files. Can be absolute or relative.",
            ShortName = "o",
            LongName = "output",
            ShowInHelpText = true)]
        public string OutputFilePath { get; set; }

        /// <summary>
        /// Command execution
        /// </summary>
        /// <param name="app">The application</param>
        /// <param name="console">Console reference</param>
        /// <returns>Return code</returns>
        public Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var sourceFolderPath = Path.GetFullPath(InputFolderPath);
            if (!fileSystem.Directory.Exists(sourceFolderPath))
            {
                console.EmitWarning(GetType(), app.Options, $"Cannot open {InputFolderPath}");
                return Task.FromResult(1);
            }

            var files = fileSystem.Directory.GetFiles(sourceFolderPath);
            foreach (var file in files)
            {
                console.WriteLine(file);
            }

            return Task.FromResult(0);
        }
    }

    public class JsonDirectoryProcessor
    {
        private IFileSystem fileSystem;

        public JsonDirectoryProcessor(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public void ProcessDirectory(string inputPath)
        {
            //read files, deserialize these, call to build string, add to string build, save output file
        }
    }

    public class EventHandlerConfigToPowerShellConverter
    {
        public async Task<string> Convert(EventHandlerConfig config)
        {
            // take input object and convert to string 
            // probably can be static

            return await Task.FromResult(string.Empty);
        }
    } 
}
