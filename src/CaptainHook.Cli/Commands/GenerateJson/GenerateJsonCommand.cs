using Eshopworld.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Cli.Commands.GenerateJson
{
    /// <summary>
    /// A command to generate a set of JSON files from a Captain Hook setup powershell script
    /// </summary>
    [Command("generateJson", Description = "generates JSON files from a Captain Hook setup powershell script"), HelpOption]
    public class GenerateJsonCommand
    {
        private readonly IBigBrother bigBrother;

        public GenerateJsonCommand(IBigBrother bigBrother)
        {
            this.bigBrother = bigBrother;
        }
    }
}
