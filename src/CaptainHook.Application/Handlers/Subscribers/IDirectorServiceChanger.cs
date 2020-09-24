using System.Threading.Tasks;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public interface IDirectorServiceChanger
    {
        Task<OperationResult<SubscriberEntity>> ApplyAsync(SubscriberEntity existingEntity, SubscriberEntity requestedEntity);
    }
}