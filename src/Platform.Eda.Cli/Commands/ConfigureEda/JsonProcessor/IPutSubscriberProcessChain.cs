using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IPutSubscriberProcessChain
    {
        /// <summary>
        /// Executes the main process pipeline
        /// </summary>
        /// <param name="inputFolderPath">The input folder</param>
        /// <param name="env">The environment name</param>
        /// <param name="replacementParams">The replacement parameters</param>
        /// <param name="noDryRun">Whether to perform a dry run or not</param>
        /// <returns>Error code</returns>
        Task<int> ProcessAsync(string inputFolderPath, string env, Dictionary<string, string> replacementParams, bool noDryRun);
    }
}