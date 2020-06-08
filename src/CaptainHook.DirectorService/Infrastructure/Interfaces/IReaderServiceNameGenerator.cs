using System.Collections.Generic;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IReaderServiceNameGenerator
    {
        string GenerateNewName(SubscriberNaming subscriber);

        IList<string> FindOldNames(SubscriberNaming subscriber, IEnumerable<string> serviceList);
    }
}
