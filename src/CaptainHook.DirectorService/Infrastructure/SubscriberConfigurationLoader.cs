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

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository, IConfigurationMerger configurationMerger)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _configurationMerger = configurationMerger ?? throw new ArgumentNullException(nameof(configurationMerger));
        }

        public async Task<(IList<WebhookConfig>, IList<SubscriberConfiguration>)> LoadAsync(string keyVaultUri)
        {
            var configuration = Configuration.Load(keyVaultUri);
            var subscribersFromKV = configuration.SubscriberConfigurations;
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();
            var merged = await _configurationMerger.MergeAsync(subscribersFromKV.Values, subscribersFromCosmos);

            return (configuration.WebhookConfigurations, merged);
        }
    }
}
