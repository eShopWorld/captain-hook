using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using McMaster.Extensions.CommandLineUtils;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    [Command("execution", Description = "Calls CaptainHook API to create/update subscribers"), HelpOption]
    public class ExecuteApiCommand
    {
        private readonly ICaptainHookClient _captainHookClient;
        public ExecuteApiCommand(ICaptainHookClient captainHookClient)
        {
            _captainHookClient = captainHookClient;
        }

        public async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
        {
            // example
            var status = await _captainHookClient.GetConfigurationStatusWithHttpMessagesAsync();

            return (int)status.Response.StatusCode;
        }
    }
}
