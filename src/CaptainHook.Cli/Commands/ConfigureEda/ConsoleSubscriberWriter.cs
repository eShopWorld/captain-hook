using System.Collections.Generic;
using System.Linq;
using CaptainHook.Cli.Commands.ConfigureEda.Models;
using McMaster.Extensions.CommandLineUtils;

namespace CaptainHook.Cli.Commands.ConfigureEda
{
    public class ConsoleSubscriberWriter
    {
        private readonly IConsole _console;

        public ConsoleSubscriberWriter(IConsole console)
        {
            _console = console;
        }

        public void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles)
        {
            var files = subscriberFiles?.ToArray();
            if (files == null || !files.Any())
            {
                _console.WriteLine("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
                return;
            }

            // ReSharper disable once PossibleNullReferenceException as it is checked in the statement above
            foreach (var file in files)
            {
                _console.WriteLine($"File '{file.File.Name}' has been found");
            }
        }
    }
}