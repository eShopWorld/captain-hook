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
            var result = new List<SubscriberConfiguration>();
            foreach (var kvModel in kvModels)
            {
                var cosmosModel = cosmosModels.SingleOrDefault(x => x.Name == kvModel.Name);

                if (cosmosModel == null)
                {
                    result.Add(kvModel);
                }
            }

            return result;
        }
    }
}
