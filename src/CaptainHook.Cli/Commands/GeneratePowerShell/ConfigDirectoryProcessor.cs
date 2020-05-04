using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using CaptainHook.Common.Configuration;
using Newtonsoft.Json;

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

            var allConfigs = new List<EventHandlerConfig>();

            var files = fileSystem.Directory.GetFiles(sourceFolderPath);
            foreach (var file in files)
            {
                var content = fileSystem.File.ReadAllText(file);

                var settings = new JsonSerializerSettings();
                settings.Converters.Add(new AuthenticationConfigConverter());
                var eventHandlerConfig = JsonConvert.DeserializeObject<EventHandlerConfig>(content, settings);
                allConfigs.Add(eventHandlerConfig);
            }

            var converter = new EventHandlerConfigToPowerShellConverter();
            var powershellCommands = converter.Convert(allConfigs);
            fileSystem.File.WriteAllLines(outputFilePath, powershellCommands);

            return Result.Valid;
        }
    }
}