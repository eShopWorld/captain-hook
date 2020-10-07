using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Extensions;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class ConsoleSubscriberWriter : IConsoleSubscriberWriter
    {
        private readonly IConsole _console;

        public ConsoleSubscriberWriter(IConsole console)
        {
            _console = console;
        }

        public void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles, string inputFolderPath)
        {
            var files = subscriberFiles?.ToArray();
            if (files == null || !files.Any())
            {
                _console.WriteWarning("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
                return;
            }

            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            foreach (var file in files)
            {
                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, file.File.FullName);
                if (file.IsError)
                {
                    _console.WriteError($"File '{fileRelativePath}' has been found, but will be skipped due to error: {file.Error}.");
                }
                else
                {
                    _console.WriteNormal($"File '{fileRelativePath}' has been found");
                }
            }
        }

        public static class Colors
        {
            public static readonly Color Red = new Color("\u001b[31m");
            public static readonly Color Green = new Color("\u001b[32m");
            public static readonly Color Cyan = new Color("\u001b[36m");
            public static readonly Color Yellow = new Color("\u001b[33m");
            public static readonly Color Reset = new Color("\u001b[0m");
        }

        public class Color
        {
            private readonly string _value;

            public Color(string value)
            {
                _value = value;
            }

            public override string ToString()
            {
                return _value;
            }
        }
    }
}