using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Models;

namespace CaptainHook.DirectorService.Infrastructure
{
    public class ConfigurationMerger
    {
        public IEnumerable<SubscriberConfiguration> Merge(IEnumerable<SubscriberConfiguration> kvModels, IEnumerable<Subscriber> cosmosModels)
        {
            var onlyInKv = kvModels.Where(k => !cosmosModels.Any(c => k.Name == c.Event.Name && k.SubscriberName == c.Name));
            //var onlyInCosmos = cosmosModels.Where(c => !kvModels.Any(k => k.Name == c.Event.Name && k.SubscriberName == c.Name));
            //var intersection = cosmosModels.Where(c => kvModels.Any(k => k.Name == c.Event.Name && k.SubscriberName == c.Name));

            var fromCosmos = cosmosModels.Select(MapSubscriber);

            var result = onlyInKv.Union(fromCosmos);
            return result;


            //var result = new List<SubscriberConfiguration>(kvModels);

            
            //foreach (var kvModel in kvModels)
            //{
            //    var cosmosModel = cosmosModels.SingleOrDefault(x => x.Name == kvModel.Name);

            //    if (cosmosModel != null)
            //    {
            //        result.Add(kvModel);
            //    }
            //}

            //return result;
        }

        private SubscriberConfiguration MapSubscriber(Subscriber cosmosModel)
        {
            var config = new SubscriberConfiguration
            {
                Name = cosmosModel.Event.Name,
                SubscriberName = cosmosModel.Name,
                Uri = cosmosModel.Webhooks.Endpoints.First().Uri
            };

            return config;
        }
    }
}
