namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IAuthHandler
    {
        Task GetToken(HttpClient client);
    }
}