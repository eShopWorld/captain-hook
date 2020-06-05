using CaptainHook.Common.Configuration;
using System.Collections.Generic;

namespace CaptainHook.DirectorService.Infrastructure.Interfaces
{
    public interface IReaderServiceNameGenerator
    {
        string GenerateNewName(SubscriberConfiguration subscriber);

        IList<string> FindOldNames(SubscriberConfiguration subscriber, IList<string> serviceList);
    }
}
