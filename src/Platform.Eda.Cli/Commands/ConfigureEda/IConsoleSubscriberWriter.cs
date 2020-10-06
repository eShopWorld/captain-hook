using System.Collections.Generic;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IConsoleSubscriberWriter
    {
        void OutputSubscribers(IEnumerable<PutSubscriberFile> subscriberFiles, string inputFolderPath);

        void WriteNormal(params string[] lines);

        void WriteSuccess(params string[] lines);

        void WriteWarning(params string[] lines);

        void WriteError(params string[] lines);

        void WriteNormalBox(params string[] lines);

        void WriteSuccessBox(params string[] lines);

        void WriteErrorBox(params string[] lines);
    }
}