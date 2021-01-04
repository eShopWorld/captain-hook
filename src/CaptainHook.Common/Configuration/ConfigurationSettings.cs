using Eshopworld.DevOps.KeyVault;

namespace CaptainHook.Common.Configuration
{
    public class ConfigurationSettings
    {
        public const string KeyVaultUriEnvVariable = "KEYVAULT_BASE_URI";

        public string AzureSubscriptionId { get; set; }

        [KeyVaultSecretName("cm--ai-telemetry--instrumentation")]
        public string InstrumentationKey { get; set; }

        [KeyVaultSecretName("cm--ai-telemetry--internal")]
        public string InternalKey { get; set; }

        public string ServiceBusConnectionString { get; set; }

        public string ServiceBusNamespace { get; set; }
    }
}
