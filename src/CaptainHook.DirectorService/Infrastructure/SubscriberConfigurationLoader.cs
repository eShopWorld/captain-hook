using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberConfigurationLoader : ISubscriberConfigurationLoader
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly ISubscriberEntityToConfigurationMapper _subscriberEntityToConfigurationMapper;

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository, ISubscriberEntityToConfigurationMapper subscriberEntityToConfigurationMapper)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _subscriberEntityToConfigurationMapper = subscriberEntityToConfigurationMapper ?? throw new ArgumentNullException(nameof(_subscriberEntityToConfigurationMapper));
        }

        public async Task<LoadingSubscribersResult> LoadAsync()
        {
            var entities = await _subscriberRepository.GetAllSubscribersAsync();
            if (entities.IsError)
            {
                return new LoadingSubscribersResult(entities.Error); ;
            }

            var configurations = await MapCosmosEntities(entities.Data);
            return configurations;
        }

        private async Task<LoadingSubscribersResult> MapCosmosEntities(IEnumerable<SubscriberEntity> subscribers)
        {
            var tasks = new List<Task<OperationResult<SubscriberConfiguration>>>();
            foreach (var entity in subscribers)
            {
                tasks.Add(TryMap(ent => _subscriberEntityToConfigurationMapper.MapToWebhookAsync(ent), entity));
                if (entity.HasDlqHooks)
                {
                    tasks.Add(TryMap(ent => _subscriberEntityToConfigurationMapper.MapToDlqAsync(ent), entity));
                }
            }

            await Task.WhenAll(tasks);

            var results = tasks.Select(x => x.Result).ToArray();

            var errors = results.Where(x => x.IsError).Select(x => x.Error);
            var configs = results.Where(r => !r.IsError).Select(r => r.Data);

            return new LoadingSubscribersResult(configs, errors);
        }

        private static Task<OperationResult<SubscriberConfiguration>> TryMap(
            Func<SubscriberEntity, Task<OperationResult<SubscriberConfiguration>>> innerFunc, SubscriberEntity entity)
        {
            var result = innerFunc(entity);

            if (result.IsFaulted)
            {
                var mappingError = new MappingError($"Cannot map SubscriberEntity to SubscriberConfiguration. SubscriberId: {entity?.Id}", new ExceptionFailure(result.Exception));
                return Task.FromResult(new OperationResult<SubscriberConfiguration>(mappingError));
            }

            return Task.FromResult(result.Result);
        }
    }
}
