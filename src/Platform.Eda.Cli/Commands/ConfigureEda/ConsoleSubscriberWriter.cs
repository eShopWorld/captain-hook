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
                WriteWarning("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
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

        public void WriteNormal(params string[] lines)
        {
            WriteInColor(Colors.Cyan, lines);
        }

        public void WriteSuccess(params string[] lines)
        {
            WriteInColor(Colors.Green, lines);
        }

        public void WriteWarning(params string[] lines)
        {
            WriteInColor(Colors.Yellow, lines);
        }

        public void WriteError(params string[] lines)
        {
            WriteInColor(Colors.Red, lines);
        }

        public void WriteNormalBox(params string[] lines)
        {
            WriteNormal(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        public void WriteSuccessBox(params string[] lines)
        {
            WriteSuccess(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        public void WriteErrorBox(params string[] lines)
        {
            WriteError(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        private void WriteInColor(Color color, params string[] lines)
        {
            foreach (var line in lines)
            {
                _console.WriteLine($"{color}{line}{Colors.Reset}");
            }
        }

        private static readonly string _boxDelimiter = new string('=', 80);

        private static class Colors
        {
            public static readonly Color Red = new Color("\u001b[31m");
            public static readonly Color Green = new Color("\u001b[32m");
            public static readonly Color Cyan = new Color("\u001b[36m");
            public static readonly Color Yellow = new Color("\u001b[33m");
            public static readonly Color Reset = new Color("\u001b[0m");
        }

        private class Color
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


        //private void WriteBox(int length) => _console.WriteLine(new string('=', length));

        //private void WriteInColor(ConsoleColor color, params string[] lines)
        //{
        //    Action writeBox = null;

        //    if (lines.Length > 1 && string.Equals("box", lines[0], StringComparison.OrdinalIgnoreCase))
        //    {
        //        var longestLine = lines.Skip(1).Max(l => l.Length);
        //        writeBox = () => WriteBox(longestLine);
        //    }

        //    var line = string.Join(Environment.NewLine, writeBox == null ? lines : lines.Skip(1));
        //    //_console.ForegroundColor = color;

        //    writeBox?.Invoke();
        //    _console.WriteLine(line);
        //    writeBox?.Invoke();
        //    _console.ResetColor();
        //}
    }
}