using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CaptainHook.Domain.Results;
using Newtonsoft.Json;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class SubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                foreach (var fileName in subscriberFiles)
                {
                    var content = _fileSystem.File.ReadAllText(fileName);
                    var request = JsonConvert.DeserializeObject<PutSubscriberRequest>(content);

                    var file = new PutSubscriberFile
                    {
                        File = new FileInfo(fileName),
                        Request = request
                    };

                    subscribers.Add(file);
                }
            }
            catch (Exception e)
            {
                return new CliExecutionError(e.ToString());
            }

            return subscribers;
        }
    }
}