using System;
using System.Collections.Generic;
using System.Text;
using CaptainHook.Domain.Results;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface ISubscribersDirectoryProcessor
    {
        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath);
    }
}
