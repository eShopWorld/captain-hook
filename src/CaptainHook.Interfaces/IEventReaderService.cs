using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using Microsoft.ServiceFabric.Services.Remoting;

namespace CaptainHook.Interfaces
{
    public interface IEventReaderService : IService
    {
        /// <summary>
        /// Completes a message, deletes the message from the service bus if the message processing is a success
        /// </summary>
        /// <param name="messageData"></param>
        /// <param name="messageDelivered"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CompleteMessageAsync(MessageData messageData, bool messageDelivered, CancellationToken cancellationToken = default);
    }
}