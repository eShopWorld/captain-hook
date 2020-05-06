using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class ConfigDirectoryProcessor
    {
        public class Result
        {
            public bool Success { get; private set; }
            public string Message { get; private set; }

            private Result()
            {
                Success = true;
            }

            public Result(string message)
            {
                Success = false;
                Message = message;
            }

            public static Result Valid = new Result();
        }

        private IFileSystem fileSystem;

        public ConfigDirectoryProcessor(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public Result ProcessDirectory(string inputFolderPath, string outputFilePath)
        {
            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            if (!fileSystem.Directory.Exists(sourceFolderPath))
            {
                return new Result($"Cannot open {inputFolderPath}");
            }

            var files = fileSystem.Directory.GetFiles(sourceFolderPath);
            var allConfigs = new List<string>();
            foreach (var file in files)
            {
                var content = fileSystem.File.ReadAllText(file);
                allConfigs.Add(content);
            }

            var powershellCommands = new EventHandlerConfigToPowerShellConverter().Convert(allConfigs);
            fileSystem.File.WriteAllLines(outputFilePath, powershellCommands);

            return Result.Valid;
        }
    }
}