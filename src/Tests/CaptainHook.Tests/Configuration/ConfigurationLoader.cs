using Eshopworld.DevOps;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Tests.Configuration
{
    /// <summary>
    /// Loads configuration parameters for Integrations tests from AppSettings.json files and secrets from the KeyvaultUrl configured in appsettings
    /// </summary>
    public class ConfigurationLoader
    {
        public static TestsConfig GetTestsConfig()
        {
            var config = EswDevOpsSdk.BuildConfiguration(); // TODO (Nikhil): When the new DevOpsSdk is available, use that to not load the whole KV
            var testsConfig = new TestsConfig();

            config.Bind(testsConfig); // Binds InstrumentationKey and AzureSubscriptionId from KV; PeterPanBaseUrl and StsClientId from appsettings
            config.Bind("CaptainHook", testsConfig); // Binds CaptainHook:ServiceBusConnectionString, CaptainHook:ApiSecret from KV

            return testsConfig;
        }
    }
}
