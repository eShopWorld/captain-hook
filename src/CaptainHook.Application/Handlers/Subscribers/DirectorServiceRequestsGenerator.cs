using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DirectorServiceRequestsGenerator : IDirectorServiceRequestsGenerator
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;

        public DirectorServiceRequestsGenerator(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
        }

        public async Task<OperationResult<IEnumerable<ReaderChangeBase>>> DefineChangesAsync(SubscriberEntity requestedEntity, SubscriberEntity existingEntity)
        {
            if (existingEntity == null)
            {
                return await _entityToConfigurationMapper.MapToWebhookAsync(requestedEntity)
                    .Then(async sc =>
                    {
                        if (requestedEntity.HasDlqHooks)
                        {
                            return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                                .Then<SubscriberConfiguration, IEnumerable<ReaderChangeBase>>(
                                    dlq => new ReaderChangeBase[] { new CreateReader { Subscriber = sc }, new CreateReader { Subscriber = dlq } });
                        }

                        return new[] { new CreateReader { Subscriber = sc } };
                    });
            }

            return await _entityToConfigurationMapper.MapToWebhookAsync(requestedEntity)
                .Then(async sc =>
                {
                    if (!existingEntity.HasDlqHooks && requestedEntity.HasDlqHooks)
                    {
                        return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                            .Then<SubscriberConfiguration, IEnumerable<ReaderChangeBase>>(
                                dlq => new ReaderChangeBase[] { new UpdateReader { Subscriber = sc }, new CreateReader { Subscriber = dlq } });
                    }

                    if (existingEntity.HasDlqHooks && !requestedEntity.HasDlqHooks)
                    {
                        return await _entityToConfigurationMapper.MapToDlqAsync(existingEntity)
                            .Then<SubscriberConfiguration, IEnumerable<ReaderChangeBase>>(
                                dlq => new ReaderChangeBase[] { new UpdateReader { Subscriber = sc }, new DeleteReader { Subscriber = dlq } });
                    }

                    return await _entityToConfigurationMapper.MapToDlqAsync(requestedEntity)
                        .Then<SubscriberConfiguration, IEnumerable<ReaderChangeBase>>(
                            dlq => new ReaderChangeBase[] { new UpdateReader { Subscriber = sc }, new UpdateReader { Subscriber = dlq } });
                });
        }
    }
}