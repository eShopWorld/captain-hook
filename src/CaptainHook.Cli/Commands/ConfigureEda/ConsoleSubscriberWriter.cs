using System.Collections.Generic;
using System.IO;
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

        public void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles, string inputFolderPath)
        {
            var files = subscriberFiles?.ToArray();
            if (files == null || !files.Any())
            {
                _console.WriteLine("No subscriber files have been found in the folder. Ensure you used the correct folder and the relevant files have the .json extensions.");
                return;
            }

            var sourceFolderPath = Path.GetFullPath(inputFolderPath);
            foreach (var file in files)
            {
                var fileRelativePath = Path.GetRelativePath(sourceFolderPath, file.File.FullName);
                _console.WriteLine($"File '{fileRelativePath}' has been found");
            }
        }

    }
}