using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface ISubscribersDirectoryProcessor
    {
        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath);
    }
}
