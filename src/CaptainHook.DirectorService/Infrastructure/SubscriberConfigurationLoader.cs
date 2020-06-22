using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Repositories;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberConfigurationLoader
    {
        private readonly ISubscriberRepository _subscriberRepository;

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository)
        {
            _subscriberRepository = subscriberRepository;
        }

        public async Task<(IList<WebhookConfig>, IList<SubscriberConfiguration>)> LoadAsync()
        {
            var configurationMerger = new ConfigurationMerger();
            var configuration = Configuration.Load();
            var subscribersFromKV = configuration.SubscriberConfigurations;
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();
            var merged = configurationMerger.Merge(subscribersFromKV.Values, subscribersFromCosmos);

            return (configuration.WebhookConfigurations, merged);
        }
    }
}
