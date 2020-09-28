using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CaptainHook.Domain.Results;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Newtonsoft.Json;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class SubscribersDirectoryParser : ISubscribersDirectoryParser
    {
        private readonly IFileSystem _fileSystem;
        private readonly IConsoleSubscriberWriter _writer;

        public SubscribersDirectoryParser(IFileSystem fileSystem, IConsoleSubscriberWriter writer)
        {
            _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath)
        {
            var sourceFolderPath = _fileSystem.Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new CliExecutionError($"Cannot open {inputFolderPath}");
            }

            try
            {
                var subscriberFiles = _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json", SearchOption.AllDirectories);
                var parsedFiles = subscriberFiles.Select(ProcessFile);

                if (!parsedFiles.Any())
                {
                    return new CliExecutionError($"No subscriber files have been found in '{sourceFolderPath}'. Ensure you used the correct folder and the relevant files have the .json extensions.");
                }

                var totalFilesCount = parsedFiles.Count();
                var successfullyParsedFilesCount = parsedFiles.Count(x => !x.IsError);

                if(successfullyParsedFilesCount == 0)
                {
                    return new CliExecutionError("No valid files to process");
                }

                _writer.WriteSuccess("box", $"Found {totalFilesCount} files, {successfullyParsedFilesCount} parsed successfully, {totalFilesCount - successfullyParsedFilesCount} with errors.");

                return parsedFiles.ToList();
            }
            catch (Exception exception)
            {
                return new CliExecutionError(exception.Message);
            }
        }

        private PutSubscriberFile ProcessFile(string fileName)
        {
            try
            {
                var content = _fileSystem.File.ReadAllText(fileName);
                var request = JsonConvert.DeserializeObject<PutSubscriberRequest>(content);
                
                _writer.WriteNormal($"File '{fileName}' has been found.");
                return new PutSubscriberFile(fileName, request);
            }
            catch (Exception exception)
            {
                _writer.WriteError($"File '{fileName}' has been found, but will be skipped due to error {exception.Message}.");
                return new PutSubscriberFile(fileName, exception.Message);
            }
        }
    }
}