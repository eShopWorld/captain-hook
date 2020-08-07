using Eshopworld.Tests.Core;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class ConfigLoaderTests
    {
        [Fact]
        [IsDev]
        public void ConfigNotEmpty()
        {
            var configuration = CaptainHook.Common.Configuration.Configuration.Load(TestConstants.TestKeyVaultUri);

            Assert.NotNull(configuration.Settings);
            Assert.NotEmpty(configuration.SubscriberConfigurations);
        }
    }
}
