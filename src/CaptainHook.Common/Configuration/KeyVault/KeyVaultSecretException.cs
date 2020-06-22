using System;
using Eshopworld.Core;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultSecretException: ExceptionEvent
    {
        public KeyVaultSecretException(Exception exception): base(exception)
        {
        }

        public string KeyVault { get; set; }

        public string SecretName { get; set; }
    }
}