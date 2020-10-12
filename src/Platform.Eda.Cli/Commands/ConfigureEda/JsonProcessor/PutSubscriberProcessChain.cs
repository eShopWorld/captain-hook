﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Domain.Results;
using Castle.Core.Internal;
using FluentValidation.Results;
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
            _console.WriteSuccessBox($"Reading files from folder: '{inputFolderPath}' to be run against {env} environment");
            var readDirectoryResult = _subscribersDirectoryProcessor.ProcessDirectory(inputFolderPath);

            if (readDirectoryResult.IsError)
            {
                _console.WriteError(readDirectoryResult.Error.Message);
                return 1;
            }

            if (readDirectoryResult.Data == null || !readDirectoryResult.Data.Any())
            {
                _console.WriteWarning("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
                return 1;
            }

            var subscriberFilePaths = readDirectoryResult.Data;
            var putSubscriberFiles = new List<PutSubscriberFile>();

            foreach (var subscriberFilePath in subscriberFilePaths)
            {
                // Step 1 - Read file
                var fileRelativePath = Path.GetRelativePath(inputFolderPath, subscriberFilePath);
                _console.WriteNormal(string.Empty, $"Processing file: '{fileRelativePath}'");

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
                if (!validationResult.IsValid)
                {
                    _console.WriteValidationResult("JSON file validation", validationResult);
                    continue;
                }

                // Step 2 - Extract vars dictionary
                JObject varsJObject = null;
                if (parsedFile.ContainsKey("vars"))
                {
                    _console.WriteNormal("Extracting variables");
                    varsJObject = (JObject)parsedFile["vars"];
                }

                // Step 2.0 - silently ignore file if it's not defined for current environment
                var environmentsResult = EnvironmentNamesExtractor.FindInVars(varsJObject);
                if (environmentsResult.IsError)
                {
                    _console.WriteError(environmentsResult.Error.Message);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = environmentsResult.Error.Message,
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }

                if (environmentsResult.Data.Any() && !environmentsResult.Data.Contains(env?.ToLower()))
                {
                    _console.WriteNormal($"File skipped due to lack of variables defined for environment `{env}'");
                    continue;
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

                // Step 2.1 - validate vars and params
                var varsValidationResult = await new FileReplacementsValidator(extractVarsResult.Data).ValidateAsync(parsedFile);
                var paramsValidationResult = await new FileReplacementsValidator(replacementParams ?? new Dictionary<string, string>()).ValidateAsync(parsedFile);
                var replacementValidationResult = new ValidationResult(varsValidationResult.Errors.Concat(paramsValidationResult.Errors));
                if (!replacementValidationResult.IsValid)
                {
                    _console.WriteValidationResult("vars and params validation", replacementValidationResult);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = string.Join(", ", replacementValidationResult.Errors),
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }

                // Step 3 - Replace vars and params
                var variablesDictionary = extractVarsResult.Data;
                var template = parsedFile.ToString();

                if (variablesDictionary.Count > 0)
                {
                    _console.WriteNormal("Replacing vars in template");
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
                    template = templateReplaceResult.Data;
                }

                if (!replacementParams.IsNullOrEmpty())
                {
                    _console.WriteNormal("Replacing params in template");

                    var paramsDictionary = new Dictionary<string, JToken>(
                        replacementParams.Select(kv => new KeyValuePair<string, JToken>(kv.Key, kv.Value)));
                    var templateReplaceResult = _jsonTemplateValuesReplacer.Replace("params", template, paramsDictionary);
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

                    template = templateReplaceResult.Data;
                }

                // Step 4 - Create PutSubscriberFile object for further processing
                _console.WriteNormal("File successfully parsed");
                putSubscriberFiles.Add(new PutSubscriberFile
                {
                    Request = JsonConvert.DeserializeObject<PutSubscriberRequest>(template),
                    File = new FileInfo(subscriberFilePath)
                });
            }

            if (noDryRun)
            {
                _console.WriteSuccess("Starting to run configuration against Captain Hook API");

                var apiResults = await ConfigureEdaWithCaptainHook(_console, inputFolderPath, env, putSubscriberFiles);
                if (apiResults.Any(r => r.IsError))
                {
                    return 2;
                }
            }
            else
            {
                _console.WriteSuccess("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
                return putSubscriberFiles.Any(file => file.IsError) ? 1 : 0;
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
                    writer.WriteError($"Error when processing '{fileRelativePath}' for event '{apiResult.Request.EventName}'," +
                                      $" subscriber '{apiResult.Request.SubscriberName}'. Error details: ", apiResultResponse.Error.Message);
                }
                else
                {
                    var operationDescription = apiResult.Response?.Data?.Response?.StatusCode switch
                    {
                        HttpStatusCode.Created => "created",
                        HttpStatusCode.Accepted => "updated",
                        _ => $"unknown result (HTTP Status {apiResult.Response?.Data?.Response?.StatusCode:D})"
                    };

                    writer.WriteNormal($"File '{fileRelativePath}' has been processed successfully. Event '{apiResult.Request.EventName}', " +
                                       $"subscriber '{apiResult.Request.SubscriberName}' has been {operationDescription}.");
                }
            }

            return apiResults;
        }
    }
}
