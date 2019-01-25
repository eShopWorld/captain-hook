using System.Net.Http;
using System.Threading.Tasks;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IRefreshTokenHandler
    {
        Task RefreshToken(HttpClient client);
    }
}
