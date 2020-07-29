﻿using Eshopworld.DevOps;
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
            var config = EswDevOpsSdk.BuildConfiguration(); // don't load whole kv
            var kvConfig = new ConfigurationBuilder().AddAzureKeyVault(
                config["KeyVaultUrl"],
                new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider().KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            this.ServiceBusConnectionString = kvConfig["CaptainHook:ServiceBusConnectionString"]; // KV
            this.StsClientSecret = kvConfig["TestConfig:Authentication:ClientSecret"]; // KV
            
            this.InstrumentationKey = kvConfig["InstrumentationKey"]; // KV
            this.SubscriptionId = kvConfig["AzureSubscriptionId"]; // KV
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
