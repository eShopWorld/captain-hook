using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Domain.Results;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.ConfigureEda
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
        [Option("-i|--input",
            Description = "The path to the folder containing JSON files to process. Can be absolute or relative",
            ShowInHelpText = true)]
        public string InputFolderPath { get; set; }

        /// <summary>
        /// The environment name.
        /// </summary>
        [Option("-e|--env",
            Description = "The environment name: (CI, TEST, PREP, SAND, PROD). Default: CI",
            ShowInHelpText = true)]
        public string Environment { get; set; }

        /// <summary>
        /// Gets or sets a flag specifying the Dry-Run mode.
        /// </summary>
        [Option("-n|--no-dry-run",
            Description = "By default the CLI is executed in the dry-run mode where no data is passed to Captain Hook API. You can disable dry-run and allow the configuration to be applied to Captain Hook API",
            ShowInHelpText = true)]
        public bool NoDryRun { get; set; }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            if (string.IsNullOrWhiteSpace(Environment))
            {
                Environment = "CI";
            }

            var writer = new ConsoleSubscriberWriter(console);

            writer.WriteSuccess("box", $"Reading files from folder: '{InputFolderPath}' to be run against {Environment} environment");

            var processor = new SubscribersDirectoryProcessor(_fileSystem);
            var readDirectoryResult = processor.ProcessDirectory(InputFolderPath, Environment);

            if (readDirectoryResult.IsError)
            {
                console.EmitWarning(GetType(), app.Options, readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberFiles = readDirectoryResult.Data;
            writer.OutputSubscribers(subscriberFiles, InputFolderPath);

            if (NoDryRun)
            {
                writer.WriteSuccess("box", "Starting to run configuration against Captain Hook API");

                var apiResults = await ConfigureEdaWithCaptainHook(writer, subscriberFiles);
                if (apiResults.Any(r => r.IsError))
                {
                    return 2;
                }
            }
            else
            {
                writer.WriteSuccess("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
            }

            writer.WriteSuccess("Processing finished");
            return 0;
        }

        private async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaWithCaptainHook(
            ConsoleSubscriberWriter writer,
            IEnumerable<PutSubscriberFile> subscriberFiles)
        {
            var api = new ApiConsumer(_captainHookClient, null);
            var apiResults = new List<OperationResult<HttpOperationResponse>>();

            var sourceFolderPath = Path.GetFullPath(InputFolderPath);
            await foreach (var apiResult in api.CallApiAsync(subscriberFiles))
            {
                var apiResultResponse = apiResult.Response;
                apiResults.Add(apiResultResponse);

                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, apiResult.File.FullName);
                if (apiResultResponse.IsError)
                {
                    writer.WriteError($"Error when processing '{fileRelativePath}':", apiResultResponse.Error.Message);
                }
                else
                {
                    writer.WriteNormal($"File '{fileRelativePath}' has been processed successfully");
                }
            }

            return apiResults;
        }
    }
}
