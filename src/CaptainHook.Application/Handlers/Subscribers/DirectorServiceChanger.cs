using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DirectorServiceChanger : IDirectorServiceChanger
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly IDirectorServiceProxy _directorService;

        public DirectorServiceChanger(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper, IDirectorServiceProxy directorService)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
            _directorService = directorService;
        }

        public async Task<OperationResult<SubscriberConfiguration>> ApplyAsync(SubscriberEntity existingEntity, SubscriberEntity requestedEntity)
        {
            if (existingEntity == null)
            {
                return await _entityToConfigurationMapper.MapToWebhookAsync(requestedEntity)
                    .Then(async webhookConfig => await _directorService.CreateReaderAsync(webhookConfig))
                    .Then(async webhookConfig => await CreateDlqReader(requestedEntity, webhookConfig));
            }

            return await _entityToConfigurationMapper.MapToWebhookAsync(requestedEntity)
                .Then(async webhookConfig => await _directorService.UpdateReaderAsync(webhookConfig))
                .Then(async webhookConfig => await UpdateDlqReader(requestedEntity, existingEntity));
        }

        private async Task<OperationResult<SubscriberConfiguration>> CreateDlqReader(SubscriberEntity requestedEntity, SubscriberConfiguration webhookConfig)
        {
            if (requestedEntity.HasDlqHooks)
            {
                return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                    .Then(async dlqConfig => await _directorService.CreateReaderAsync(dlqConfig));
            }

            return webhookConfig;
        }

        private async Task<OperationResult<SubscriberConfiguration>> UpdateDlqReader(SubscriberEntity requestedEntity, SubscriberEntity existingEntity)
        {
            if (!existingEntity.HasDlqHooks && requestedEntity.HasDlqHooks)
            {
                return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                    .Then(async dlqConfig => await _directorService.CreateReaderAsync(dlqConfig));
            }

            if (existingEntity.HasDlqHooks && !requestedEntity.HasDlqHooks)
            {
                return await _entityToConfigurationMapper.MapToDlqAsync(existingEntity)
                    .Then(async dlqConfig => await _directorService.DeleteReaderAsync(dlqConfig));
            }

            return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                .Then(async dlqConfig => await _directorService.UpdateReaderAsync(dlqConfig));
        }
    }
}