using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;

namespace CaptainHook.DirectorService.ReaderServiceManagement
{
    public class ReaderServiceChangesDetector : IReaderServiceChangesDetector
    {
        public IEnumerable<ReaderChangeInfo> DetectChanges(IEnumerable<SubscriberConfiguration> newSubscribers, IEnumerable<string> deployedServicesNames)
        {
            // Describe the situation
            var desiredReaders = newSubscribers.Select(c => new DesiredReaderDefinition(c)).ToArray();
            var existingReaders = deployedServicesNames
                .Where(ExistingReaderDefinition.IsValidReaderService)
                .Select(s => new ExistingReaderDefinition(s))
                .ToArray();

            var result = existingReaders
                .Where(e => desiredReaders.All(d => !d.IsTheSameService(e)))
                .Select(ReaderChangeInfo.ToBeRemoved)
                .ToList();

            foreach (var newReader in desiredReaders)
            {
                var existing = existingReaders.FirstOrDefault(e => newReader.IsTheSameService(e));
                if (!existing.IsValid)
                    result.Add(ReaderChangeInfo.ToBeCreated(newReader));
                else if (!newReader.IsUnchanged(existing))
                    result.Add(ReaderChangeInfo.ToBeUpdated(newReader, existing));
            }

            return result;
        }
    }
}