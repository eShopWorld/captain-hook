using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DirectorServiceChanger : IDirectorServiceChanger
    {
        private readonly IDirectorServiceProxy _directorService;

        public DirectorServiceChanger(IDirectorServiceProxy directorService)
        {
            _directorService = directorService;
        }

        public async Task<OperationResult<SubscriberEntity>> ApplyAsync(SubscriberEntity existingEntity, SubscriberEntity requestedEntity)
        {
            if (existingEntity == null)
            {
                return await _directorService.CreateReaderAsync(requestedEntity)
                    .Then(async webhookConfiguration => await CreateDlqReaderAsync(requestedEntity, webhookConfiguration))
                    .Then<SubscriberConfiguration, SubscriberEntity>(_ => requestedEntity);
            }

            return await _directorService.UpdateReaderAsync(requestedEntity)
                .Then(async webhookConfig => await ChangeDlqReaderAsync(requestedEntity, existingEntity))
                .Then<SubscriberConfiguration, SubscriberEntity>(_ => requestedEntity);
        }

        private async Task<OperationResult<SubscriberConfiguration>> CreateDlqReaderAsync(SubscriberEntity requestedEntity, SubscriberConfiguration webhookConfiguration)
        {
            if (requestedEntity.HasDlqHooks)
            {
                return await _directorService.CreateDlqReaderAsync(requestedEntity);
            }

            return webhookConfiguration;
        }

        private async Task<OperationResult<SubscriberConfiguration>> ChangeDlqReaderAsync(SubscriberEntity requestedEntity, SubscriberEntity existingEntity)
        {
            if (!existingEntity.HasDlqHooks && requestedEntity.HasDlqHooks)
            {
                return await _directorService.CreateDlqReaderAsync(requestedEntity);
            }

            if (existingEntity.HasDlqHooks && !requestedEntity.HasDlqHooks)
            {
                return await _directorService.DeleteDlqReaderAsync(existingEntity);
            }

            return await _directorService.UpdateDlqReaderAsync(requestedEntity);
        }
    }
}