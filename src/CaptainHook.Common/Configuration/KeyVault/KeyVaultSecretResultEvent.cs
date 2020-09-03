using Eshopworld.Core;

namespace CaptainHook.Common.Configuration.KeyVault
{
    public class KeyVaultSecretResultEvent : TelemetryEvent
    {
        public string KeyVaultName { get; set; }
        public string SecretName { get; set; }
        public string ResponseReason { get; set; }
        public string SecretValue { get; set; }
    }
}
