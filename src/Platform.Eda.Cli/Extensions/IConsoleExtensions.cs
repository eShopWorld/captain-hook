﻿using System.IO;
using System.Linq;
using FluentValidation.Results;
using McMaster.Extensions.CommandLineUtils;

namespace Platform.Eda.Cli.Extensions
{
    /// <summary>
    /// console extensions to support warnings and errors and associated telemetry
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IConsoleExtensions
    {
        private static readonly string BoxDelimiter = new string('=', 80);
        private const int IndentSize = 5;
        
        private const string Ok = "Ok";
        private const string Skip = "Skip";
        private const string Error = "Error";

        public static class Colors
        {
            public static readonly Color Red = new Color("\u001b[31m");
            public static readonly Color Green = new Color("\u001b[32m");
            public static readonly Color Yellow = new Color("\u001b[33m");
            public static readonly Color Default = new Color(string.Empty);
            public static readonly Color Reset = new Color("\u001b[0m");
        }

        public static void WriteNormal(this IConsole console, params string[] lines)
        {
            console.Out.WriteInColor(Colors.Default, lines);
        }

        public static void WriteNormalWithFileName(this IConsole console, string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Ok, fileName, lines);
            WriteNormal(console, lines);
        }

        public static void WriteSuccess(this IConsole console, params string[] lines)
        {
            console.Out.WriteInColor(Colors.Green, lines);
        }

        public static void WriteWarning(this IConsole console, params string[] lines)
        {
            console.Out.WriteInColor(Colors.Yellow, lines);
        }

        public static void WriteSkippedWithFileName(this IConsole console, string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Skip, fileName, lines);

            WriteWarning(console, lines);
        }

        public static void WriteError(this IConsole console, params string[] lines)
        {
            console.Out.WriteInColor(Colors.Red, lines);
        }

        public static void WriteErrorWithFileName(this IConsole console, string fileName, params string[] lines)
        {
            lines = PrepareIndentedStrings(Error, fileName, lines);

            WriteError(console, lines);
        }

        public static void WriteNormalBox(this IConsole console, params string[] lines)
        {
            console.WriteNormal(lines.Prepend(BoxDelimiter).Append(BoxDelimiter).ToArray());
        }

        public static void WriteSuccessBox(this IConsole console, params string[] lines)
        {
            console.WriteSuccess(lines.Prepend(BoxDelimiter).Append(BoxDelimiter).ToArray());
        }

        public static void WriteWarningBox(this IConsole console, params string[] lines)
        {
            console.WriteWarning(lines.Prepend(BoxDelimiter).Append(BoxDelimiter).ToArray());
        }

        public static void WriteErrorBox(this IConsole console, params string[] lines)
        {
            console.WriteError(lines.Prepend(BoxDelimiter).Append(BoxDelimiter).ToArray());
        }

        public static void WriteValidationResultWithFileName(this IConsole console, string fileName, string stageName, ValidationResult validationResult)
        {
            var failures = validationResult.Errors.Select((failure, i) => $"{i + 1}. {failure.ErrorMessage}").ToArray();
            console.WriteErrorWithFileName(fileName, failures.Prepend($"Validation errors during {stageName} - failures:").ToArray());
        }

        private static void WriteInColor(this TextWriter writer, Color color, params string[] lines)
        {
            foreach (var line in lines)
            {
                writer.WriteLine($"{color}{line}{Colors.Reset}");
            }
        }

        private static string[] PrepareIndentedStrings(string result, string fileName, string[] lines)
        {
            var header = $"{result,-IndentSize} > {fileName}";
            lines = lines
                .Select(line => $"{string.Empty,-IndentSize} | {line}")
                .Prepend(header)
                .ToArray();
            return lines;
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
