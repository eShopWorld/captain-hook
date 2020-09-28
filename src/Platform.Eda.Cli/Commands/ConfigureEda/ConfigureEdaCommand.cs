using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [Command("configure-eda", Description = "Processes configuration in the provided location and calls Captain Hook API to create/update subscribers")]
    [HelpOption]
    public class ConfigureEdaCommand
    {
        private readonly ISubscribersDirectoryParser _subscribersDirectoryParser;
        private readonly ISubscribersProcessor _subscribersProcessor;

        public ConfigureEdaCommand(ISubscribersDirectoryParser subscribersDirectoryParser, ISubscribersProcessor subscribersProcessor)
        {
            _subscribersDirectoryParser = subscribersDirectoryParser ?? throw new ArgumentNullException(nameof(subscribersDirectoryParser));
            _subscribersProcessor = subscribersProcessor ?? throw new ArgumentNullException(nameof(subscribersProcessor));
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

        public async Task<int> OnExecuteAsync(IConsoleSubscriberWriter writer)
        {
            if (string.IsNullOrWhiteSpace(Environment))
            {
                Environment = "CI";
            }

            writer.WriteSuccess("box", $"Reading files from folder: '{InputFolderPath}' to be run against {Environment} environment");

            var readDirectoryResult = _subscribersDirectoryParser.ProcessDirectory(InputFolderPath);

            if (readDirectoryResult.IsError)
            {
                writer.WriteError(readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberRequests = readDirectoryResult.Data.Where(x => !x.IsError);

            if (NoDryRun)
            {
                writer.WriteSuccess("box", "Starting to run configuration against Captain Hook API");

                var apiResults = await _subscribersProcessor.ConfigureEdaAsync(subscriberRequests, Environment);

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
    }
}
