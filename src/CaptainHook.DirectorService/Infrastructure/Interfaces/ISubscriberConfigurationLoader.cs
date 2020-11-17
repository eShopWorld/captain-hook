using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface ISubscriberConfigurationLoader
    {
        Task<LoadingSubscribersResult> LoadAsync();
    }
}