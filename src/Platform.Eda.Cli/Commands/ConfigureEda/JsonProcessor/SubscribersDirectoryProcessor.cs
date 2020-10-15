using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using CaptainHook.Domain.Results;
using Platform.Eda.Cli.Common;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public class SubscribersDirectoryProcessor : ISubscribersDirectoryProcessor
    {
        private readonly IFileSystem _fileSystem;

        public SubscribersDirectoryProcessor(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public OperationResult<IEnumerable<string>> ProcessDirectory(string inputFolderPath)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!_fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new CliExecutionError($"Cannot open {inputFolderPath}");
            }

            try
            {
                var subscriberFiles =
                    _fileSystem.Directory.GetFiles(sourceFolderPath, "*.json", SearchOption.AllDirectories);

                return subscriberFiles;
            }
            catch (Exception e)
            {
                return new CliExecutionError(e.ToString());
            }
        }
    }
}