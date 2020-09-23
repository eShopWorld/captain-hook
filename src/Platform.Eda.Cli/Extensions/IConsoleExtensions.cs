using System;
using System.Collections.Generic;
using System.IO;
using McMaster.Extensions.CommandLineUtils;

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
    }
}
