using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    public class KeyVaultConfigurationError : ErrorBase
    {
        public KeyVaultConfigurationError(IEnumerable<KeyVaultConfigurationFailure> failures)
            : base("Can't load KeyVault configuration for subscribers.")
        {
            Failures = failures.ToArray();
        }
    }
}