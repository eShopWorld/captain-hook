using CaptainHook.Domain.Results;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface ISubscribersProcessor
    {
        /// <summary>
        /// Configures EDA with the provided subscriber files
        /// </summary>
        /// <param name="subscriberFiles"></param>
        /// <returns></returns>
        Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaAsync(IEnumerable<PutSubscriberFile> subscriberFiles, string environment);
    }
}
