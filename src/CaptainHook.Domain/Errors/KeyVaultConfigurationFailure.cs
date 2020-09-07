using System;
using CaptainHook.Domain.Results;

namespace CaptainHook.Domain.Errors
{
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