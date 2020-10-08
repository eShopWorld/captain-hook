using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
using Castle.Core.Internal;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Rest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Eda.Cli.Commands.ConfigureEda.JsonValidation;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class PutSubscriberProcessChain : IPutSubscriberProcessChain
    {
        private readonly ISubscribersDirectoryProcessor _subscribersDirectoryProcessor;
        private readonly ISubscriberFileParser _subscriberFileParser;
        private readonly IJsonVarsExtractor _jsonVarsExtractor;
        private readonly IJsonTemplateValuesReplacer _jsonTemplateValuesReplacer;
        private readonly IConsole _console;
        private readonly BuildCaptainHookProxyDelegate _captainHookBuilder;

        public PutSubscriberProcessChain(
            IConsole console,
            ISubscribersDirectoryProcessor subscribersDirectoryProcessor,
            ISubscriberFileParser subscriberFileParser,
            IJsonVarsExtractor jsonVarsExtractor,
            IJsonTemplateValuesReplacer subscriberTemplateReplacer,
            BuildCaptainHookProxyDelegate captainHookBuilder)
        {
            _console = console ?? throw new ArgumentNullException(nameof(console));
            _subscribersDirectoryProcessor = subscribersDirectoryProcessor ?? throw new ArgumentNullException(nameof(subscribersDirectoryProcessor));
            _subscriberFileParser = subscriberFileParser ?? throw new ArgumentNullException(nameof(subscriberFileParser));
            _jsonVarsExtractor = jsonVarsExtractor ?? throw new ArgumentNullException(nameof(jsonVarsExtractor));
            _jsonTemplateValuesReplacer = subscriberTemplateReplacer ?? throw new ArgumentNullException(nameof(subscriberTemplateReplacer));
            _captainHookBuilder = captainHookBuilder ?? throw new ArgumentNullException(nameof(captainHookBuilder));
        }

        public async Task<int> ProcessAsync(string inputFolderPath, string env, Dictionary<string, string> replacementParams, bool noDryRun)
        {
            _console.WriteSuccessBox("box", $"Reading files from folder: '{inputFolderPath}' to be run against {env} environment");
            var readDirectoryResult = _subscribersDirectoryProcessor.ProcessDirectory(inputFolderPath);

            if (readDirectoryResult.IsError)
            {
                _console.WriteError(readDirectoryResult.Error.Message);
                return 1;
            }

            var subscriberFilePaths = readDirectoryResult.Data;
            var putSubscriberFiles = new List<PutSubscriberFile>();

            foreach (var subscriberFilePath in subscriberFilePaths)
            {
                // Step 1 - Read file
                var parseFileResult = _subscriberFileParser.ParseFile(subscriberFilePath);
                if (parseFileResult.IsError)
                {
                    _console.WriteError(parseFileResult.Error.Message);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = parseFileResult.Error.Message,
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }

                var parsedFile = parseFileResult.Data;

                // Step 1.1 - validate file
                var validationResult = await new FileStructureValidator().ValidateAsync(parsedFile);
                if (! validationResult.IsValid)
                {
                    var errors = validationResult.Errors.Select((failure, i) => $"{i + 1}. {failure.ErrorMessage}");
                    var validationMessages = new[] { "Some validation errors have been found" }.Concat(errors).ToArray();
                    _console.WriteError(validationMessages);
                }

                // Step 2 - Extract vars dictionary
                JObject varsJObject = null;
                if (parsedFile.ContainsKey("vars"))
                {
                    varsJObject = (JObject)parsedFile["vars"];
                    parsedFile.Remove("vars");
                }

                var extractVarsResult = _jsonVarsExtractor.ExtractVars(varsJObject, env);
                if (extractVarsResult.IsError)
                {
                    _console.WriteError(extractVarsResult.Error.Message);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = extractVarsResult.Error.Message,
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }

                // Step 3 - Replace vars and params
                var template = parsedFile.ToString();
                var templateReplaceResult = _jsonTemplateValuesReplacer.Replace("vars", template, extractVarsResult.Data);
                if (templateReplaceResult.IsError)
                {
                    _console.WriteError(templateReplaceResult.Error.Message);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = templateReplaceResult.Error.Message,
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }


                if (!replacementParams.IsNullOrEmpty())
                {
                    var paramsDictionary = new Dictionary<string, JToken>(
                        replacementParams.Select(kv => new KeyValuePair<string, JToken>(kv.Key, kv.Value)));
                    templateReplaceResult = _jsonTemplateValuesReplacer.Replace("params", templateReplaceResult.Data, paramsDictionary);
                    if (templateReplaceResult.IsError)
                    {
                        _console.WriteError(templateReplaceResult.Error.Message);
                        putSubscriberFiles.Add(new PutSubscriberFile
                        {
                            Error = templateReplaceResult.Error.Message,
                            File = new FileInfo(subscriberFilePath)
                        });
                        continue;
                    }
                }

                // Step 4 - Create PutSubscriberFile object for further processing
                putSubscriberFiles.Add(new PutSubscriberFile
                {
                    Request = JsonConvert.DeserializeObject<PutSubscriberRequest>(templateReplaceResult.Data),
                    File = new FileInfo(subscriberFilePath)
                });

            }

            if (noDryRun)
            {
                _console.WriteSuccess("box", "Starting to run configuration against Captain Hook API");

                var apiResults = await ConfigureEdaWithCaptainHook(_console, inputFolderPath, env, putSubscriberFiles);
                if (apiResults.Any(r => r.IsError))
                {
                    return 2;
                }
            }
            else
            {
                _console.WriteSuccess("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
            }

            return 0;
        }

        private async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaWithCaptainHook(IConsole writer,
            string inputFolderPath, string env, IEnumerable<PutSubscriberFile> subscriberFiles)
        {
            var api = _captainHookBuilder(env);
            var apiResults = new List<OperationResult<HttpOperationResponse>>();

            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            await foreach (var apiResult in api.CallApiAsync(subscriberFiles.Where(f => !f.IsError)))
            {
                var apiResultResponse = apiResult.Response;
                apiResults.Add(apiResultResponse);

                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, apiResult.File.FullName);
                if (apiResultResponse.IsError)
                {
                    _console.WriteError($"Error when processing '{fileRelativePath}' for event '{apiResult.Request.EventName}'," +
                                      $" subscriber '{apiResult.Request.SubscriberName}'. Error details: ", apiResultResponse.Error.Message);
                }
                else
                {
                    string operationDescription = apiResult?.Response?.Data?.Response?.StatusCode switch
                    {
                        HttpStatusCode.Created => "created",
                        HttpStatusCode.Accepted => "updated",
                        _ => $"unknown result (HTTP Status {apiResult?.Response?.Data?.Response?.StatusCode:D})"
                    };

                    _console.WriteNormal($"File '{fileRelativePath}' has been processed successfully. Event '{apiResult.Request.EventName}', " +
                                       $"subscriber '{apiResult.Request.SubscriberName}' has been {operationDescription}.");
                }
            }

            return apiResults;
        }
    }
}
