using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberConfigurationLoader : ISubscriberConfigurationLoader
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IConfigurationMerger _configurationMerger;
        private readonly IConfigurationLoader _configurationLoader;

        public SubscriberConfigurationLoader(
            ISubscriberRepository subscriberRepository,
            IConfigurationMerger configurationMerger,
            IConfigurationLoader configurationLoader)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _configurationMerger = configurationMerger ?? throw new ArgumentNullException(nameof(configurationMerger));
            _configurationLoader = configurationLoader ?? throw new ArgumentNullException(nameof(configurationLoader));
        }

        public async Task<IList<SubscriberConfiguration>> LoadAsync(string keyVaultUri)
        {
            var configuration = _configurationLoader.Load(keyVaultUri);
            var subscribersFromKV = configuration.SubscriberConfigurations;
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();
            
            return await _configurationMerger.MergeAsync(subscribersFromKV.Values, subscribersFromCosmos);
        }
    }
}
