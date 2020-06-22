using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using System.Collections.Generic;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IConfigurationMerger
    {
        IEnumerable<SubscriberConfiguration> Merge(IEnumerable<SubscriberConfiguration> subscribersFromKeyVault, IEnumerable<SubscriberEntity> subscribersFromCosmos);
    }
}