using System.Threading.Tasks;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public interface IApiConsumer
    {
        /// <summary>
        /// Invokes the Captain Hook API
        /// </summary>
        /// <param name="file">Subscriber file</param>
        /// <returns>API operation result</returns>
        Task<ApiOperationResult> CallApiAsync(PutSubscriberFile file);
    }
}
