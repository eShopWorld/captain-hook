using System.Collections.Generic;

namespace CaptainHook.Common.Configuration
{
    public interface ISubscribersKeyVaultProvider
    {
        IDictionary<string, SubscriberConfiguration> Load(string keyVaultUri);
    }
}