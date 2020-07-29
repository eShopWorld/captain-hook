using System.Collections.Generic;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    /// <summary>
    /// Provides functionality to detect what changes should be applied to particular reader services
    /// </summary>
    public interface IReaderServiceChangesDetector
    {
        /// <summary>
        /// Based on provided list of new subscriber configurations and list of existing service names,
        /// it will detect what changes should be applied to particular reader services and return the change set
        /// </summary>
        /// <param name="newSubscribers">List of all subscriber configurations</param>
        /// <param name="deployedServicesNames">List of all existing, deployed services for the application</param>
        /// <returns>List of changes that should be applied based on given state</returns>
        IEnumerable<ReaderChangeInfo> DetectChanges (IEnumerable<SubscriberConfiguration> newSubscribers, IEnumerable<string> deployedServicesNames);
    }
}