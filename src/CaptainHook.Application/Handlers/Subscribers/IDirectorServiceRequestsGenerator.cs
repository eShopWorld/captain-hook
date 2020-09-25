using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public interface IDirectorServiceRequestsGenerator
    {
        Task<OperationResult<IEnumerable<ReaderChangeBase>>> DefineChangesAsync(SubscriberEntity requestedEntity, SubscriberEntity existingEntity);
    }
}