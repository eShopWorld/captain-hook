using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IConfigurationMerger
    {
        ReadOnlyCollection<SubscriberConfiguration> Merge(IEnumerable<SubscriberConfiguration> subscribersFromKeyVault, IEnumerable<SubscriberEntity> subscribersFromCosmos);
    }
}