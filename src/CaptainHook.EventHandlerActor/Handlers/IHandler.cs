using System.Collections.Generic;

namespace CaptainHook.EventHandlerActor.Handlers
{
    using System.Threading.Tasks;

    public interface IHandler
    {
        Task Call<TRequest>(TRequest request, IDictionary<string, string> metaData = null);
    }
}
