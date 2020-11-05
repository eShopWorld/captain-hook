﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> LoadAsync()
        {
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();
            if (subscribersFromCosmos.IsError)
            {
                return subscribersFromCosmos.Error;
            }

            var fromCosmosResult = await MapCosmosEntities(subscribersFromCosmos.Data);

            if (fromCosmosResult.IsError)
            {
                return fromCosmosResult.Error;
            }

            return new ReadOnlyCollection<SubscriberConfiguration>(fromCosmosResult.Data);
        }

        private async Task<OperationResult<IList<SubscriberConfiguration>>> MapCosmosEntities(IEnumerable<SubscriberEntity> subscribers)
        {
            var tasks = new List<Task<OperationResult<SubscriberConfiguration>>>();
            foreach (var entity in subscribers)
            {
                tasks.Add(_subscriberEntityToConfigurationMapper.MapToWebhookAsync(entity));
                if (entity.HasDlqHooks)
                {
                    tasks.Add(_subscriberEntityToConfigurationMapper.MapToDlqAsync(entity));
                }
            }

            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                return new MappingError("Cannot map Cosmos DB entries", new ExceptionFailure(ex));
            }

            var errors = tasks.Select(x => x.Result).Where(x => x.IsError);
            if (errors.Any())
            {
                var failures = errors.SelectMany(x => x.Error.Failures).ToArray();
                return new MappingError("Cannot map Cosmos DB entries", failures);
            }

            return tasks.Select(t => t.Result.Data).ToList();
        }
    }
}
