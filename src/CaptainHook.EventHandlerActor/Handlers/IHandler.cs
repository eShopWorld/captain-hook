using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHandler
    {
        Task CallAsync<TRequest>(TRequest request, IDictionary<string, object> metaData, CancellationToken cancellationToken);
    }
}
