using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

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
                WriteNormal("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
                return;
            }

            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            foreach (var file in files)
            {
                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, file.File.FullName);
                if (file.IsError)
                {
                    WriteError($"File '{fileRelativePath}' has been found, but will be skipped due to error: {file.Error}.");
                }
                else
                {
                    WriteNormal($"File '{fileRelativePath}' has been found");
                }
            }
        }

        public void WriteNormal(params string[] lines) => WriteInColor(_console.ForegroundColor, lines);

        public void WriteSuccess(params string[] lines) => WriteInColor(ConsoleColor.DarkGreen, lines.Select(s => $"##[section] {s}").ToArray());

        public void WriteError(params string[] lines) => WriteInColor(ConsoleColor.Red, lines.Select(s => $"##[error] {s}").ToArray());

        private void WriteBox(int length) => _console.WriteLine(new string('=', length));

        private void WriteInColor(ConsoleColor color, params string[] lines)
        {
            Action writeBox = null;

            if (lines.Length > 1 && string.Equals("box", lines[0], StringComparison.OrdinalIgnoreCase))
            {
                var longestLine = lines.Skip(1).Max(l => l.Length);
                writeBox = () => WriteBox(longestLine);
            }

            var line = string.Join(Environment.NewLine, writeBox == null ? lines : lines.Skip(1));
            _console.ForegroundColor = color;

            writeBox?.Invoke();
            _console.WriteLine(line);
            writeBox?.Invoke();
            _console.ResetColor();
        }
    }
}