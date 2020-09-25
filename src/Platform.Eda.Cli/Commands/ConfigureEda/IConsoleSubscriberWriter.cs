using System.Collections.Generic;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IConsoleSubscriberWriter
    {
        public void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles, string inputFolderPath);

        public void WriteNormal(params string[] lines);

        public void WriteSuccess(params string[] lines);

        public void WriteError(params string[] lines);
    }
}