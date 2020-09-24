using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public interface IDirectorServiceChanger
    {
        Task<OperationResult<SubscriberConfiguration>> ApplyAsync(SubscriberEntity existingEntity, SubscriberEntity requestedEntity);
    }
}