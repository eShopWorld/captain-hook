namespace CaptainHook.Common
{
    public class ConfigurationSettings
    {
        public const string KeyVaultUriEnvVariable = "KEYVAULT_BASE_URI";

        public string AzureSubscriptionId { get; set; }

        public string InstrumentationKey { get; set; }

        public string ServiceBusConnectionString { get; set; }

        public string ServiceBusNamespace { get; set; }

        public string OMSCallback { get; set; }

        public string SecurityClientId { get; set; }

        public string SecurityClientSecret { get; set; }

        public string SecurityClientURI { get; set; }

        public string MAXCallback { get; set; }

        //todo move to own config and kv
        public string MAXClientId { get; set; }

        public string MAXClientSecret { get; set; }

        public string MAXAuthURI { get; set; }

        public string DIFCallback { get; set; }

        public string DIFClientId { get; set; }

        public string DIFClientSecret { get; set; }

        public string DIFAuthURI{ get; set; }
    }
}
