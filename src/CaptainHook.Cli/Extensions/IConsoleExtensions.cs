using CaptainHook.Cli.Telemetry;
using Eshopworld.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.IO;

namespace CaptainHook.Cli.Extensions
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
        /// <param name="bb"><see cref="IBigBrother"/></param>
        /// <param name="command">command type</param>
        /// <param name="args">application options</param>
        /// <param name="warning">warning message</param>
        public static void EmitWarning(this IConsole console, Type command, List<CommandOption> args, string warning)
        {
            var @event = new CaptainHookWarningEvent()
            {
                CommandType = command.ToString(),
                Arguments = args.ToConsoleString(),
                Warning = warning
            };

            var argsMessage = string.IsNullOrWhiteSpace(@event.Arguments) ? "" : $",Arguments '{@event.Arguments}'";
            console.EmitMessage(console.Out, $"WARNING - Command {@event.CommandType}{argsMessage} - {warning}");
        }

        // ReSharper disable once UnusedParameter.Local
        internal static void EmitMessage(this IConsole _, TextWriter tw, string text)
        {
            tw.WriteLine(text);
        }

        /// <summary>
        /// emit error to console and BigBrother
        /// </summary>
        /// <param name="console">console instance</param>
        /// <param name="bigBrother"><see cref="IBigBrother"/></param>
        /// <param name="exception">exception</param>
        /// <param name="command">command type</param>
        /// <param name="args">command arguments</param>
        public static void EmitException(this IConsole console, Exception exception, Type command, List<CommandOption> args)
        {
            var @event = exception.ToExceptionEvent<CaptainHookExceptionEvent>();
            @event.CommandType = command.ToString();
            @event.Arguments = args.ToConsoleString();

            var argsMessage = string.IsNullOrWhiteSpace(@event.Arguments) ? "" : $",Arguments '{@event.Arguments}'";
            console.EmitMessage(console.Error, $"ERROR - Command {@event.CommandType}{argsMessage} - {exception.GetType().FullName} - {exception.Message}");
        }
    }
}
