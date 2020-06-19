using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class SubscriberConfigurationLoader
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly ConfigurationMerger _configurationMerger;

        public SubscriberConfigurationLoader(ISubscriberRepository subscriberRepository)
        {
            _subscriberRepository = subscriberRepository;
            _configurationMerger = new ConfigurationMerger();
        }

        public async Task LoadAsync()
        {
            var configuration = Configuration.Load();

            var subscribersFromKV = configuration.SubscriberConfigurations;

            var subscribersFromCosmos = (await _subscriberRepository.GetAllSubscribersAsync())
                .ToDictionary(x => x.Name);

            Dictionary<string, SubscriberConfiguration> result = new Dictionary<string, SubscriberConfiguration>();
            

            //_configurationMerger.Merge(configuration.SubscriberConfigurations, )

        }
    }
}
