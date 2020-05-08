using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;

namespace CaptainHook.Cli.Commands.GeneratePowerShell
{
    public class ConfigDirectoryProcessor
    {
        private const string HeaderFileName = "header.ps1";
        private static readonly Regex Regex = new Regex(@"event-(\d+)-");

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

            var eventFiles = fileSystem.Directory.GetFiles(sourceFolderPath, "event-*");
            var allConfigs = new SortedDictionary<int, string>();
            foreach (var fileName in eventFiles)
            {
                int eventId = ExtractEventId(fileName);
                var content = fileSystem.File.ReadAllText(fileName);
                allConfigs.Add(eventId, content);
            }

            var powershellCommands = new EventHandlerConfigToPowerShellConverter().Convert(allConfigs);
            
            var headerFileContents = GetHeaderFileIfExists(sourceFolderPath);
            if (headerFileContents != null)
            {
                powershellCommands = new[] { headerFileContents }.Concat(powershellCommands).ToArray();
            }

            fileSystem.File.WriteAllLines(outputFilePath, powershellCommands);

            return Result.Valid;
        }

        private int ExtractEventId(string fileName)
        {
            var match = Regex.Match(fileName);
            var rawNumber = match.Groups[1].Value;
            int result = int.Parse(rawNumber);
            return result;
        }

        private string GetHeaderFileIfExists(string sourceFolderPath)
        {
            var templateFileName = Path.Combine(sourceFolderPath, HeaderFileName);
            var headerFileExists = fileSystem.File.Exists(templateFileName);
            if (headerFileExists)
            {
                return fileSystem.File.ReadAllText(templateFileName);
            }

            return null;
        }
    }
}