using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    [ExcludeFromCodeCoverage]
    public class KeyVaultConfigurationError : ErrorBase
    {
        public KeyVaultConfigurationError(IEnumerable<KeyVaultConfigurationFailure> failures)
            : base("Can't load KeyVault configuration for subscribers.")
        {
            Failures = failures.ToArray();
        }
    }
}