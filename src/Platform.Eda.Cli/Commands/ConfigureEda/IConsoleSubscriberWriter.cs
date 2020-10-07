using System.Collections.Generic;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IConsoleSubscriberWriter
    {
        void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles, string inputFolderPath);
    }
}