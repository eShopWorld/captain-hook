using System;
using System.Diagnostics.CodeAnalysis;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
    [ExcludeFromCodeCoverage]
    public class KeyVaultConfigurationFailure : IFailure
    {
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