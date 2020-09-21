using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.GeneratePowerShell
{
    [Command("generate-powershell", Description = "generates Captain Hook setup PowerShell script from  JSON files"), HelpOption]
    public class GeneratePowerShellCommand
    {
        private readonly IFileSystem fileSystem;

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
            var directoryProcessor = new ConfigDirectoryProcessor(this.fileSystem);
            var result = directoryProcessor.ProcessDirectory(InputFolderPath, OutputFilePath);

            if (result != ConfigDirectoryProcessor.Result.Valid)
            {
                console.EmitWarning(GetType(), app.Options, result.Message);
                return Task.FromResult(1);
            }

            return Task.FromResult(0);
        }
    }
}
