using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Commands.ConfigureEda.OptionsValidation;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [Command("configure-eda", Description = "Processes configuration in the provided location and calls Captain Hook API to create/update subscribers")]
    [HelpOption]
    public class ConfigureEdaCommand
    {
        private readonly ISubscribersDirectoryProcessor _subscribersDirectoryProcessor;
        private readonly BuildCaptainHookProxyDelegate _captainHookBuilder;

        public ConfigureEdaCommand(
            ISubscribersDirectoryProcessor subscribersDirectoryProcessor,
            BuildCaptainHookProxyDelegate captainHookBuilder)
        {
            _subscribersDirectoryProcessor = subscribersDirectoryProcessor ?? throw new ArgumentNullException(nameof(subscribersDirectoryProcessor));
            _captainHookBuilder = captainHookBuilder ?? throw new ArgumentNullException(nameof(captainHookBuilder));
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

        /// <summary>
        /// The environment name.
        /// </summary>
        [Option("-p|--params", CommandOptionType.MultipleValue,
            Description = "The additional configuration parameters",
            ShowInHelpText = true)]
        [ReplacementParamsDictionary]
        public string[] Params { get; set; }

        public async Task<int> OnExecuteAsync(IConsoleSubscriberWriter writer)
        {
            if (string.IsNullOrWhiteSpace(Environment))
            {
                Environment = "CI";
            }

            writer.WriteSuccessBox($"Reading files from folder: '{InputFolderPath}' to be run against {Environment} environment");

            var replacements = BuildParametersReplacementDictionary(Params);

            var readDirectoryResult = _subscribersDirectoryProcessor.ProcessDirectory(InputFolderPath);

            if (readDirectoryResult.IsError)
            {
                writer.WriteError(readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberFiles = readDirectoryResult.Data;
            writer.OutputSubscribers(subscriberFiles, InputFolderPath);

            if (NoDryRun)
            {
                writer.WriteSuccessBox("Starting to run configuration against Captain Hook API");

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

        private Dictionary<string, string> BuildParametersReplacementDictionary(string[] rawParams)
        {
            return rawParams?.Select(p => p.Split('=')).ToDictionary(items => items[0], items => items[1]);
        }

        private async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaWithCaptainHook(IConsoleSubscriberWriter writer,
            IEnumerable<PutSubscriberFile> subscriberFiles)
        {
            var api = _captainHookBuilder(Environment);
            var apiResults = new List<OperationResult<HttpOperationResponse>>();

            var sourceFolderPath = Path.GetFullPath(InputFolderPath);
            await foreach (var apiResult in api.CallApiAsync(subscriberFiles.Where(f => !f.IsError)))
            {
                var apiResultResponse = apiResult.Response;
                apiResults.Add(apiResultResponse);

                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, apiResult.File.FullName);
                if (apiResultResponse.IsError)
                {
                    writer.WriteError($"Error when processing '{fileRelativePath}' for event '{apiResult.Request.EventName}'," +
                        $" subscriber '{apiResult.Request.SubscriberName}'. Error details: ", apiResultResponse.Error.Message);
                }
                else
                {
                    string operationDescription = apiResult.Response.Data.Response.StatusCode switch
                    {
                        HttpStatusCode.Created => "created",
                        HttpStatusCode.Accepted => "updated",
                        _ => $"unknown result (HTTP Status {apiResult.Response.Data.Response.StatusCode:D})"
                    };

                    writer.WriteNormal($"File '{fileRelativePath}' has been processed successfully. Event '{apiResult.Request.EventName}', " +
                                       $"subscriber '{apiResult.Request.SubscriberName}' has been {operationDescription}.");
                }
            }

            return apiResults;
        }
    }
}
