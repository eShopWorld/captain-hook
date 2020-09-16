using System;
using System.ComponentModel.DataAnnotations;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Cli.Extensions;
using McMaster.Extensions.CommandLineUtils;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    [Command("configure-eda", Description = "Processes configuration in the provided location and calls Captain Hook API to create/update subscribers")]
    [HelpOption]
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
        [Required]
        [Option(
            Description = "The path to the folder containing JSON files to process. Can be absolute or relative",
            ShortName = "i",
            LongName = "input",
            ShowInHelpText = true)]
        public string InputFolderPath { get; set; }

        /// <summary>
        /// The environment name.
        /// </summary>
        [Required]
        [Option(
            Description = "The environment name",
            ShortName = "env",
            LongName = "environment",
            ShowInHelpText = true)]
        public string EnvironmentName { get; set; }

        /// <summary>
        /// Gets or sets a flag specifying the Dry-Run mode.
        /// </summary>
        [Option(
            Description = "Executed the dry-run mode where no data is passed to Captain Hook API",
            LongName = "dry-run",
            ShowInHelpText = true)]
        public bool DryRun { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            console.WriteLine($"Reading files from folder: '{InputFolderPath}'");

            var processor = new SubscribersDirectoryProcessor(_fileSystem);
            var writer = new ConsoleSubscriberWriter(console);
            var readDirectoryResult = processor.ProcessDirectory(InputFolderPath, EnvironmentName);

            if (readDirectoryResult.IsError)
            {
                console.EmitWarning(GetType(), app.Options, readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberFiles = readDirectoryResult.Data;
            writer.OutputSubscribers(subscriberFiles);

            if (false)
            {
                var api = new ApiConsumer(_captainHookClient);
                var apiResult = await api.CallApiAsync(subscriberFiles.Select(f => f.Request));
                if (apiResult.IsError)
                {
                    console.EmitWarning(GetType(), app.Options, apiResult.Error.Message);
                    return 2;
                }
            }

            console.ForegroundColor = ConsoleColor.DarkGreen;
            console.WriteLine("Processing finished");
            console.ResetColor();
            return 0;
        }
    }
}
