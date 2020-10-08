using System.Collections.Generic;
using CaptainHook.Domain.Results;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface ISubscribersDirectoryProcessor
    {
        /// <summary>
        /// Returns paths to all the JSON files in the target directory and its subdirectories.
        /// </summary>
        /// <param name="inputFolderPath"></param>
        /// <returns></returns>
        public OperationResult<IEnumerable<string>> ProcessDirectory(string inputFolderPath);
    }
}
