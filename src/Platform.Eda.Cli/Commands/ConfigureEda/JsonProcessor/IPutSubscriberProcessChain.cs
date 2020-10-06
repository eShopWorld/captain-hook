using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platform.Eda.Cli.Commands.ConfigureEda.JsonProcessor
{
    public interface IPutSubscriberProcessChain
    {
        Task<int> Process(string inputFolderPath, string env, Dictionary<string, string> replacementParams, bool noDryRun);
    }
}