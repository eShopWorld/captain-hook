using Microsoft.Extensions.Configuration;

namespace CaptainHook.Cli.ConfigurationProvider
{
    public class EswPsFileConfigurationSource : FileConfigurationSource
    {
        /// <summary>
        /// Builds the <see cref="EswPsFileConfigurationProvider"/> for this source.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/>.</param>
        /// <returns>An <see cref="EswPsFileConfigurationProvider"/></returns>
        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new EswPsFileConfigurationProvider(this);
        }
    }
}
