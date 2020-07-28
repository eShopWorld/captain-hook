using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Common.Configuration;
using CaptainHook.DirectorService.Infrastructure.Interfaces;
using CaptainHook.Common.Configuration.KeyVault;
using CaptainHook.Domain.Entities;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger : IConfigurationMerger
    {
        private readonly SubscriberEntityToConfigurationMapper _subscriberEntityToConfigurationMapper;

        public ConfigurationMerger(ISecretProvider secretProvider)
        {
            _subscriberEntityToConfigurationMapper = new SubscriberEntityToConfigurationMapper(secretProvider);
        }

        /// <summary>
        /// Merges subscribers loaded from Cosmos and from KeyVault. If particular subscriber is defined in both sources, the Cosmos version overrides the KeyVault version.
        /// </summary>
        /// <param name="subscribersFromKeyVault">Subscriber definitions loaded from KeyVault</param>
        /// <param name="subscribersFromCosmos">Subscriber models retrieved from Cosmos</param>
        /// <returns>List of all subscribers converted to KeyVault structure</returns>
        public async Task<ReadOnlyCollection<SubscriberConfiguration>> MergeAsync(
            IEnumerable<SubscriberConfiguration> subscribersFromKeyVault,
            IEnumerable<SubscriberEntity> subscribersFromCosmos)
        {
            var onlyInKv = subscribersFromKeyVault
                .Where(kvSubscriber => !subscribersFromCosmos.Any(cosmosSubscriber =>
                    kvSubscriber.EventType.Equals(cosmosSubscriber.ParentEvent.Name, StringComparison.InvariantCultureIgnoreCase)
                    && kvSubscriber.SubscriberName.Equals(cosmosSubscriber.Name, StringComparison.InvariantCultureIgnoreCase)));

            async Task<IEnumerable<SubscriberConfiguration>> MapCosmosEntries()
            {
                var tasks = subscribersFromCosmos.Select(_subscriberEntityToConfigurationMapper.MapSubscriber).ToArray();
                await Task.WhenAll(tasks);

                return tasks.SelectMany(t => t.Result).ToArray();
            }

            var fromCosmos = await MapCosmosEntries();
            var union = onlyInKv.Union(fromCosmos).ToList();
            return new ReadOnlyCollection<SubscriberConfiguration>(union);
        }
    }
}
