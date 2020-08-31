using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberConfigurationLoader : ISubscriberConfigurationLoader
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IConfigurationMerger _configurationMerger;

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository, IConfigurationMerger configurationMerger)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _configurationMerger = configurationMerger ?? throw new ArgumentNullException(nameof(configurationMerger));
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> LoadAsync(string keyVaultUri)
        {
            var configuration = Configuration.Load(keyVaultUri);
            var subscribersFromKV = configuration.SubscriberConfigurations;
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();

            var mergeResult = await _configurationMerger.MergeAsync(subscribersFromKV.Values, subscribersFromCosmos.Data);

            if(mergeResult.IsError)
            {
                return mergeResult.Error;
            }

            return mergeResult.Data;
        }
    }
}
