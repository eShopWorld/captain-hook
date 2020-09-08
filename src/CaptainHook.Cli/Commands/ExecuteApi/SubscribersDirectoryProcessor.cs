using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CaptainHook.Cli.Commands.ExecuteApi.Models;
using CaptainHook.Domain.Results;
using Newtonsoft.Json;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class SubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public OperationResult<IEnumerable<PutSubscriberRequest>> ProcessDirectory(string inputFolderPath, string environmentName)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new CliError($"Cannot open {inputFolderPath}");
            }

            var subscribers = new List<PutSubscriberRequest>();

            try
            {
                var subscriberFiles = _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json");
                foreach (var fileName in subscriberFiles)
                {
                    var content = _fileSystem.File.ReadAllText(fileName);
                    var request = JsonConvert.DeserializeObject<PutSubscriberRequest>(content);
                    subscribers.Add(request);
                }
            }
            catch (Exception e)
            {
                return new CliError(e.ToString());
            }

            return subscribers;
        }
    }
}