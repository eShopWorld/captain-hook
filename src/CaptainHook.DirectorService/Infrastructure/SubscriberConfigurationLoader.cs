﻿using CaptainHook.Common.Configuration;
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

        public async Task<(IEnumerable<WebhookConfig>, IEnumerable<SubscriberConfiguration>)> LoadAsync()
        {
            var configuration = Configuration.Load();
            var subscribersFromKV = configuration.SubscriberConfigurations;
            var subscribersFromCosmos = await _subscriberRepository.GetAllSubscribersAsync();
            var merged = _configurationMerger.Merge(subscribersFromKV.Values, subscribersFromCosmos);

            return (configuration.WebhookConfigurations, merged);
        }
    }
}
