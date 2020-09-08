using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Cli.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    [Command("execute-api", Description = "Calls CaptainHook API to create/update subscribers"), HelpOption]
    public class ExecuteApiCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICaptainHookClient _captainHookClient;

        public ExecuteApiCommand(IFileSystem fileSystem, ICaptainHookClient captainHookClient)
        {
            _fileSystem = fileSystem;
            _captainHookClient = captainHookClient;
        }

        /// <summary>
        /// The path to the input folder containing JSON files. Can be absolute or relative.
        /// </summary>
        [Option(
            Description = "The path to the folder containing JSON files to process. Can be absolute or relative.",
            ShortName = "i",
            LongName = "input",
            ShowInHelpText = true)]
        [Required]
        public string InputFolderPath { get; set; }


        /// <summary>
        /// The environment name.
        /// </summary>
        [Option(
            Description = "The environment name.",
            ShortName = "env",
            LongName = "environment",
            ShowInHelpText = true)]
        [Required]
        public string EnvironmentName { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            var processor = new SubscribersDirectoryProcessor(_fileSystem);
            var readDirectoryResult = processor.ProcessDirectory(InputFolderPath, EnvironmentName);

            if (readDirectoryResult.IsError)
            {
                console.EmitWarning(GetType(), app.Options, readDirectoryResult.Error.Message);
            }
            else
            {
                var api = new ApiConsumer(_captainHookClient);
                var apiResult = await api.CallApiAsync(readDirectoryResult.Data);
                if (apiResult.IsError)
                    console.EmitWarning(GetType(), app.Options, apiResult.Error.Message);
                else
                    console.WriteLine("Operation succeeded!");
            }

            return 0;
        }
    }
}
