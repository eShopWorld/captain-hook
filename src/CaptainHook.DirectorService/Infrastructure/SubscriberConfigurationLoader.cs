using System;
using System.Collections.Generic;
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
        private readonly ISubscribersKeyVaultProvider _subscribersKeyVaultProvider;

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository, IConfigurationMerger configurationMerger, ISubscribersKeyVaultProvider subscribersKeyVaultProvider)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _configurationMerger = configurationMerger ?? throw new ArgumentNullException(nameof(configurationMerger));
            _subscribersKeyVaultProvider = subscribersKeyVaultProvider ?? throw new ArgumentNullException(nameof(subscribersKeyVaultProvider));
        }

        public async Task<OperationResult<IEnumerable<SubscriberConfiguration>>> LoadAsync(string keyVaultUri)
        {
            var subscribersFromKV = _subscribersKeyVaultProvider.Load(keyVaultUri);
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();

            return (await _configurationMerger.MergeAsync(subscribersFromKV.Values, subscribersFromCosmos.Data))
                .Then<IEnumerable<SubscriberConfiguration>>(x => x);
        }
    }
}
