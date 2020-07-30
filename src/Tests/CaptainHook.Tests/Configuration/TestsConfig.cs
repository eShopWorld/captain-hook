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
    /// <summary>
    /// Class to hold configuration values for Integration Tests. 
    /// </summary>
    /// <remarks>
    /// The field names must match with the appsettings and keyvault config names for the loader to work.
    /// </remarks>
    public class TestsConfig
    {
        public string InstrumentationKey { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public string AzureSubscriptionId { get; set; }
        public string PeterPanBaseUrl { get; set; }

        /// <summary>
        /// The StsClientSecret value
        /// </summary>
        public string ApiSecret { get; set; }
        public string StsClientId { get; set; }
    }
}
