using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface ISubscribersDirectoryParser
    {
        /// <summary>
        /// Parses the files in a directory sub tree
        /// </summary>
        /// <param name="inputFolderPath">The path to the input folder</param>
        /// <returns></returns>
        public OperationResult<IEnumerable<PutSubscriberFile>> ProcessDirectory(string inputFolderPath);
    }
}
