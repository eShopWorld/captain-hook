using System.Net.Http;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAcquireTokenHandler
    {
        Task GetToken(HttpClient client);
    }
}
