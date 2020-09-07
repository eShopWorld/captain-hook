using System;
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

    public class KeyVaultConfigurationFailure : FailureBase
    {
        public override string Id => Path;
        public string Path { get; set; }
        public Exception Exception { get; set; }

        public KeyVaultConfigurationFailure(string path, Exception exception)
        {
            Path = path;
            Exception = exception;
        }

        public override string ToString()
        {
            return $"Path: '{Path}' Exception: {Exception}";
        }
    }
}