using Eshopworld.Core;
using System;

namespace CaptainHook.Cli.Telemetry
{
    public sealed class CaptainHookExceptionEvent : ExceptionEvent
    {
        /// <summary>
        /// name of command that produced the exception
        /// </summary>
        public string CommandType { get; set; }

        /// <summary>
        /// arguments passed to the command
        /// </summary>
        public string Arguments { get; set; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="exception">outer exception reference</param>
        public CaptainHookExceptionEvent(Exception exception) : base(exception)
        {
        }
    }
}
