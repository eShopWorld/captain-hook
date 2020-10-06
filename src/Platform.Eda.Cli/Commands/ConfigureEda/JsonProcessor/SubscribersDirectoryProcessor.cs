using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CaptainHook.Domain.Results;
using Newtonsoft.Json;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscribersDirectoryProcessor : ISubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;
        private readonly ISubscriberFileParser _subscriberFileParser;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem, ISubscriberFileParser subscriberFileParser)
        {
            _fileSystem = fileSystem;
            _subscriberFileParser = subscriberFileParser;
        }

        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new CliExecutionError($"Cannot open {inputFolderPath}");
            }

            var subscribers = new List<PutSubscriberFile>();
            try
            {
                var subscriberFiles = _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json", SearchOption.AllDirectories);

                if (!subscriberFiles.Any())
                {
                    return new CliExecutionError($"No files have been found in '{sourceFolderPath}'");
                }

                foreach (var fileName in subscriberFiles)
                {
                    var fileResult = ProcessFile(fileName);
                    subscribers.Add(fileResult);
                }
            }
            catch (Exception e)
            {
                return new CliExecutionError(e.ToString());
            }

            return subscribers;
        }

        private PutSubscriberFile ProcessFile(string fileName)
        {
            try
            {
                var content = _fileSystem.File.ReadAllText(fileName);
                var request = JsonConvert.DeserializeObject<PutSubscriberRequest>(content);

                return new PutSubscriberFile
                {
                    File = new FileInfo(fileName),
                    Request = request
                };
            }
            catch (Exception ex)
            {
                return new PutSubscriberFile
                {
                    File = new FileInfo(fileName),
                    Error = ex.Message
                };
            }
        }
    }
}