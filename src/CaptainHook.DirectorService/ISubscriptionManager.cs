using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService
{
    public interface ISubscriptionManager
    {
        public Task CreateServicesAsync(CancellationToken cancellationToken);
    }
}
