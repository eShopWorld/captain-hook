using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using McMaster.Extensions.CommandLineUtils;
using Platform.Eda.Cli.Commands.ConfigureEda;

namespace Platform.Eda.Cli.Extensions
{
    /// <summary>
    /// console extensions to support warnings and errors and associated telemetry
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static class IConsoleExtensions
    {
        /// <summary>
        /// emit warning to console and BigBrother
        /// </summary>
        /// <param name="console">extension instance</param>
        /// <param name="command">command type</param>
        /// <param name="args">application options</param>
        /// <param name="warning">warning message</param>
        public static void EmitWarning(this IConsole console, Type command, List<CommandOption> args, string warning)
        {
            var argsMessage = string.IsNullOrWhiteSpace(args.ToConsoleString()) ? string.Empty : $",Arguments '{args.ToConsoleString()}'";
            console.EmitMessage(console.Out, $"WARNING - Command {command}{argsMessage} - {warning}");
        }

        internal static void EmitMessage(this IConsole _, TextWriter textWriter, string text)
        {
            textWriter.WriteLine(text);
        }

        /// <summary>
        /// emit error to console and BigBrother
        /// </summary>
        /// <param name="console">console instance</param>
        /// <param name="exception">exception</param>
        /// <param name="command">command type</param>
        /// <param name="args">command arguments</param>
        public static void EmitException(this IConsole console, Exception exception, Type command, List<CommandOption> args)
        {
            var argsMessage = string.IsNullOrWhiteSpace(args.ToConsoleString()) ? string.Empty : $", Arguments '{args.ToConsoleString()}'";
            console.EmitMessage(console.Error, $"ERROR - Command {command}{argsMessage} - {exception.GetType().FullName} - {exception.Message}");
        }


        public static void WriteNormal(this IConsole console, params string[] lines)
        {
            console.WriteInColor(Colors.Cyan, lines);
        }

        public static void WriteSuccess(this IConsole console, params string[] lines)
        {
            console.WriteInColor(Colors.Green, lines);
        }

        public static void WriteWarning(this IConsole console, params string[] lines)
        {
            console.WriteInColor(Colors.Yellow, lines);
        }

        public static void WriteError(this IConsole console, params string[] lines)
        {
            console.WriteInColor(Colors.Red, lines);
        }

        public static void WriteNormalBox(this IConsole console, params string[] lines)
        {
            console.WriteNormal(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        public static void WriteSuccessBox(this IConsole console, params string[] lines)
        {
            console.WriteSuccess(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        public static void WriteErrorBox(this IConsole console, params string[] lines)
        {
            console.WriteError(lines.Prepend(_boxDelimiter).Append(_boxDelimiter).ToArray());
        }

        private static void WriteInColor(this IConsole console, Color color, params string[] lines)
        {
            foreach (var line in lines)
            {
                console.WriteLine($"{color}{line}{Colors.Reset}");
            }
        }

        private static readonly string _boxDelimiter = new string('=', 80);

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
