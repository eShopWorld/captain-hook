using System.Collections.Generic;
using CaptainHook.Domain.Results;

namespace CaptainHook.Common.Configuration
{
    public interface ISubscribersKeyVaultProvider
    {
        OperationResult<IDictionary<string, SubscriberConfiguration>> Load(string keyVaultUri);
    }
}