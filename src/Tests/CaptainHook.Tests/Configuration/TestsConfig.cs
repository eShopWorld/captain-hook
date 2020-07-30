using Eshopworld.DevOps;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Tests.Configuration
{
    public class TestsConfig
    {
        /// <summary>
        /// Loads configuration parameters for Integrations tests from AppSettings.json files and secrets from the KeyvaultUrl configured in appsettings
        /// </summary>
        public TestsConfig()
        {
            var config = EswDevOpsSdk.BuildConfiguration(); // TODO (Nikhil): When the new DevOpsSdk is available, use that to not load the whole KV

            ServiceBusConnectionString = config["CaptainHook:ServiceBusConnectionString"]; // KV
            StsClientSecret = config["CaptainHook:ApiSecret"]; // KV
            
            InstrumentationKey = config["InstrumentationKey"]; // KV
            SubscriptionId = config["AzureSubscriptionId"]; // KV
            PeterPanUrlBase = config["TestConfig:PeterPanBaseUrl"]; // AS
            StsClientId = config["TestConfig:Authentication:StsClientId"]; // AS
        }
        public string InstrumentationKey { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string SubscriptionId { get; set; }
        public string PeterPanUrlBase { get; set; }
        public string StsClientSecret { get; set; }
        public string StsClientId { get; set; }
    }
}
