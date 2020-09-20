using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Cli.Commands.ConfigureEda.Models;
using CaptainHook.Cli.Extensions;
using CaptainHook.Domain.Results;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;

namespace CaptainHook.Cli.Commands.ConfigureEda
{
    [Command("configure-eda", Description = "Processes configuration in the provided location and calls Captain Hook API to create/update subscribers")]
    [HelpOption]
    public class ConfigureEdaCommand
    {
        private readonly IFileSystem _fileSystem;
        private readonly ICaptainHookClient _captainHookClient;

        public ConfigureEdaCommand(IFileSystem fileSystem, ICaptainHookClient captainHookClient)
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
            Description =
                "By default the CLI is executed in the dry-run mode where no data is passed to Captain Hook API. You can disable dry-run and allow the configuration to be applied to Captain Hook API",
            LongName = "no-dry-run",
            ShowInHelpText = true)]
        public bool NoDryRun { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            WriteInGreen("=============================================");
            WriteInGreen($"Reading files from folder: '{InputFolderPath}'");
            WriteInGreen("=============================================");

            var processor = new SubscribersDirectoryProcessor(_fileSystem);
            var writer = new ConsoleSubscriberWriter(console);
            var readDirectoryResult = processor.ProcessDirectory(InputFolderPath, EnvironmentName);

            if (readDirectoryResult.IsError)
            {
                console.EmitWarning(GetType(), app.Options, readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberFiles = readDirectoryResult.Data;
            writer.OutputSubscribers(subscriberFiles, InputFolderPath);

            if (NoDryRun)
            {
                WriteInGreen("=============================================");
                WriteInGreen("Starting to run configuration against Captain Hook API");
                WriteInGreen("=============================================");

                var apiResults = await ConfigureEdaWithCaptainHook(app, console, subscriberFiles);
                if (apiResults.Any(r => r.IsError))
                {
                    return 2;
                }
            }
            else
            {
                WriteInGreen("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
            }

            WriteInGreen("Processing finished");
            return 0;

            void WriteInGreen(string writeLine)
            {
                console.ForegroundColor = ConsoleColor.DarkGreen;
                console.WriteLine(writeLine);
                console.ResetColor();
            }
        }

        private async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaWithCaptainHook(
            CommandLineApplication app,
            IConsole console,
            IEnumerable<PutSubscriberFile> subscriberFiles)
        {
            var api = new ApiConsumer(_captainHookClient);
            var apiResults = new List<OperationResult<HttpOperationResponse>>();

            var sourceFolderPath = Path.GetFullPath(InputFolderPath);
            await foreach (var apiResult in api.CallApiAsync(subscriberFiles))
            {
                var apiResultResponse = apiResult.Response;
                apiResults.Add(apiResultResponse);

                if (apiResultResponse.IsError)
                {
                    console.EmitWarning(GetType(), app.Options, apiResultResponse.Error.Message);
                }
                else
                {
                    var fileRelativePath = Path.GetRelativePath(sourceFolderPath, apiResult.File.FullName);
                    console.WriteLine($"File '{fileRelativePath}' has been processed successfully");
                }
            }

            return apiResults;
        }
    }
}
