using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IFabricClientWrapper
    {
        Task<List<string>> GetServiceUriListAsync();

        Task CreateServiceAsync(ServiceCreationDescription serviceCreationDescription, CancellationToken cancellationToken);

        Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken);
    }

    
}
