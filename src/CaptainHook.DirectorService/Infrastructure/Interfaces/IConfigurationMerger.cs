using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IConfigurationMerger
    {
        Task<OperationResult<ReadOnlyCollection<SubscriberConfiguration>>> MergeAsync(
            IEnumerable<SubscriberConfiguration> subscribersFromKeyVault,
            IEnumerable<SubscriberEntity> subscribersFromCosmos);
    }
}