using System.Collections.Generic;
using System.Linq;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class ConfigLoaderTests
    {
        [Fact]
        [IsDev]
        public void ConfigNotEmpty()
        {
            var configuration = Common.Configuration.Configuration.Load();

            Assert.NotNull(configuration.Settings);
            Assert.NotEmpty(configuration.SubscriberConfigurations);
            Assert.NotEmpty(configuration.WebhookConfigurations);
        }
    }
}
