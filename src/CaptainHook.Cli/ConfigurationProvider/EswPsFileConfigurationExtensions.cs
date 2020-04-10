using Microsoft.Extensions.Configuration;
using System;

namespace CaptainHook.Cli.ConfigurationProvider
{
    /// <summary>
    /// Extension methods for adding <see cref="EswPsFileConfigurationExtensions"/>.
    /// </summary>
    public static class EswPsFileConfigurationExtensions
    {
        /// <summary>
        /// Adds the PS file configuration provider at <paramref name="path"/> to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="path">Path relative to the base path stored in
        /// <see cref="IConfigurationBuilder.Properties"/> of <paramref name="builder"/>.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEswPsFile(this IConfigurationBuilder builder, string path)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("Invalid file path", nameof(path));
            }

            return builder.AddEswPsFile(s =>
            {
                s.FileProvider = null;
                s.Path = path;
                s.Optional = false;
                s.ReloadOnChange = false;
                s.ResolveFileProvider();
            });
        }

        /// <summary>
        /// Adds a PS file configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEswPsFile(this IConfigurationBuilder builder, Action<EswPsFileConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
