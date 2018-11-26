namespace CaptainHook.Common.Authentication
{
    using System.Threading.Tasks;

    public interface IAccessTokenHandler
    {
        Task<string> GetToken();
    }
}