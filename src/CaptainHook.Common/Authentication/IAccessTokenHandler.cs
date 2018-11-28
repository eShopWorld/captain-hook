namespace CaptainHook.Common.Authentication
{
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IAccessTokenHandler
    {
        Task GetToken(HttpClient client);
    }
}