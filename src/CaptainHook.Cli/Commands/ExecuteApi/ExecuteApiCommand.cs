using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    [Command("execution", Description = "Calls CaptainHook API to create/update subscribers"), HelpOption]
    public class ExecuteApiCommand
    {
         public Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
         {
             return Task.FromResult(1);
         }
    }
}
