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

            this.ServiceBusConnectionString = config["CaptainHook:ServiceBusConnectionString"]; // KV
            this.StsClientSecret = config["TestConfig:Authentication:ClientSecret"]; // KV
            
            this.InstrumentationKey = config["InstrumentationKey"]; // KV
            this.SubscriptionId = config["AzureSubscriptionId"]; // KV
            this.PeterPanUrlBase = config["TestConfig:PeterPanBaseUrl"]; // AS
            this.StsClientId = config["TestConfig:Authentication:StsClientId"]; // AS
        }
        public String InstrumentationKey { get; set; }
        public String ServiceBusConnectionString { get; set; }
        public String SubscriptionId { get; set; }
        public String PeterPanUrlBase { get; set; }
        public String StsClientSecret { get; set; }
        public String StsClientId { get; set; }
    }
}
