using System.Collections.Generic;
using CaptainHook.Domain.Results;

namespace CaptainHook.Common.Configuration
{
    public interface ISubscribersKeyVaultProvider
    {
        OperationResult<IEnumerable<SubscriberConfiguration>> Load(string keyVaultUri);
    }
}