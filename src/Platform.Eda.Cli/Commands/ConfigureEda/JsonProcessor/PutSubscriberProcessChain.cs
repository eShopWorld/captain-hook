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

        private const int IndentSize = 5;

        private const string Ok = "Ok";
        private const string Skip = "Skip";
        private const string Error = "Error";

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
                return 0;
            }

            var subscriberFilePaths = readDirectoryResult.Data;
            var putSubscriberFiles = new List<PutSubscriberFile>();

            foreach (var subscriberFilePath in subscriberFilePaths)
            {
                // Step 1 - Read file
                var fileRelativePath = Path.GetRelativePath(inputFolderPath, subscriberFilePath);

                var parseFileResult = _subscriberFileParser.ParseFile(subscriberFilePath);
                if (parseFileResult.IsError)
                {
                    WriteErrorProcessingResult(fileRelativePath, parseFileResult.Error.Message);
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
                    WriteValidationResult(fileRelativePath, "JSON file validation", validationResult);
                    continue;
                }

                // Step 2.0 - silently ignore file if it's not defined for current environment
                var varsJObject = (JObject)parsedFile["vars"];
                var environmentsResult = EnvironmentNamesExtractor.FindInVars(varsJObject);
                if (environmentsResult.IsError)
                {
                    WriteErrorProcessingResult(fileRelativePath, environmentsResult.Error.Message);
                    putSubscriberFiles.Add(new PutSubscriberFile
                    {
                        Error = environmentsResult.Error.Message,
                        File = new FileInfo(subscriberFilePath)
                    });
                    continue;
                }

                if (environmentsResult.Data.Any() && !environmentsResult.Data.Contains(env?.ToLower()))
                {
                    WriteSkippedFileResult(fileRelativePath);
                    continue;
                }

                // Step 2 - Extract vars dictionary
                var extractVarsResult = _jsonVarsExtractor.ExtractVars(varsJObject, env);
                if (extractVarsResult.IsError)
                {
                    WriteErrorProcessingResult(fileRelativePath, extractVarsResult.Error.Message);
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
                    WriteValidationResult(fileRelativePath, "vars and params validation", replacementValidationResult);
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
                    var templateReplaceResult = _jsonTemplateValuesReplacer.Replace("vars", template, extractVarsResult.Data);
                    if (templateReplaceResult.IsError)
                    {
                        WriteErrorProcessingResult(fileRelativePath, templateReplaceResult.Error.Message);
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
                    var paramsDictionary = new Dictionary<string, JToken>(
                        replacementParams!.Select(kv => new KeyValuePair<string, JToken>(kv.Key, kv.Value)));
                    var templateReplaceResult = _jsonTemplateValuesReplacer.Replace("params", template, paramsDictionary);
                    if (templateReplaceResult.IsError)
                    {
                        WriteErrorProcessingResult(fileRelativePath, templateReplaceResult.Error.Message);
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
                if (!noDryRun) // output File Ok only on dry run
                {
                    WriteNormalProcessingResult(fileRelativePath);
                }

                putSubscriberFiles.Add(new PutSubscriberFile
                {
                    Request = JsonConvert.DeserializeObject<PutSubscriberRequest>(template),
                    File = new FileInfo(subscriberFilePath)
                });
            }

            if (noDryRun)
            {
                _console.WriteNormalBox("Starting to run configuration against Captain Hook API");

                var apiResults = await ConfigureEdaWithCaptainHook(inputFolderPath, env, putSubscriberFiles);
                if (apiResults.Any(r => r.IsError))
                {
                    return 2;
                }
            }
            else
            {
                _console.WriteNormalBox("By default the CLI runs in 'dry-run' mode. If you want to run the configuration against Captain Hook API use the '--no-dry-run' switch");
                return putSubscriberFiles.Any(file => file.IsError) ? 1 : 0;
            }

            return 0;
        }

        private async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaWithCaptainHook(string inputFolderPath, string env, IEnumerable<PutSubscriberFile> subscriberFiles)
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
                    WriteErrorProcessingResult(
                        $"Event: '{apiResult.Request.EventName}', Subscriber: '{apiResult.Request.SubscriberName}', File: {fileRelativePath}.",
                        apiResultResponse.Error.Message.Split(Environment.NewLine));
                }
                else
                {
                    var operationDescription = apiResult.Response?.Data?.Response?.StatusCode switch
                    {
                        HttpStatusCode.Created => "created",
                        HttpStatusCode.Accepted => "updated",
                        _ => $"unknown result (HTTP Status {apiResult.Response?.Data?.Response?.StatusCode:D})"
                    };

                    WriteNormalProcessingResult(
                        $"{operationDescription} Event '{apiResult.Request.EventName}', Subscriber: '{apiResult.Request.SubscriberName}', File: {fileRelativePath}.");
                }
            }

            return apiResults;
        }

        public void WriteSkippedFileResult(string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Skip, fileName, lines);

            _console.WriteWarning(lines);
        }

        private void WriteErrorProcessingResult(string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Error, fileName, lines);

            _console.WriteError(lines);
        }

        private void WriteValidationResult(string fileName, string stageName, ValidationResult validationResult)
        {
            var failures = validationResult.Errors.Select((failure, i) => $"{i + 1}. {failure.ErrorMessage}").ToArray();
            WriteErrorProcessingResult(fileName, failures.Prepend($"Validation errors during {stageName} - failures:").ToArray());
        }

        private void WriteNormalProcessingResult(string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Ok, fileName, lines);
            _console.WriteNormal(lines);
        }

        private static string[] PrepareIndentedStrings(string result, string fileName, params string[] lines)
        {
            var header = $"{result,-IndentSize} > {fileName}";
            lines = lines
                .Select(line => $"{string.Empty,-IndentSize} | {line}")
                .Prepend(header)
                .ToArray();
            return lines;
        }
    }
}
