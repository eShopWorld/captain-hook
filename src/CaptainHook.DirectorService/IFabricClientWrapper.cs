using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService
{
    public interface IFabricClientWrapper
    {
        Task<List<string>> GetServiceUriListAsync();

        Task CreateServiceAsync(string serviceName, CancellationToken cancellationToken);

        Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken);
    }
}
