using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService
{
    public interface IReaderServicesManager
    {
        /// <summary>
        /// Creates new instance of readers. Also deletes obsolete and no longer configured ones.
        /// </summary>
        /// <param name="subscribers">List of subscribers to create</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="webhooks">List of webhook configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task CreateReadersAsync(IEnumerable<SubscriberConfiguration> subscribers, IEnumerable<string> deployedServicesNames, IEnumerable<WebhookConfig> webhooks, CancellationToken cancellationToken);

        /// <summary>
        /// Compares newly read Configuration with list of currently deployed subscribers and based on that create new, delete old
        /// and refresh (by pair of create and delete operation) existing readers.
        /// </summary>
        /// <param name="newConfiguration">Target Configuration to be deployed</param>
        /// <param name="deployedServicesNames">List of currently deployed services names</param>
        /// <param name="currentSubscribers">List of currently deployed subscribers</param>
        /// <returns></returns>
        Task RefreshReadersAsync(IDictionary<string, SubscriberConfiguration> newSubscribers, IEnumerable<WebhookConfig> newWebhooks, 
            IDictionary<string, SubscriberConfiguration> currentSubscribers, IEnumerable<string> deployedServicesNames, CancellationToken cancellationToken);
    }
}