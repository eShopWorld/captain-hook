using System;
using System.IO.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Platform.Eda.Cli.Providers
{
    /// <summary>
    /// Extension methods for adding <see cref="EswPsFileConfigurationExtensions"/>.
    /// </summary>
    public static class EswPsFileConfigurationExtensions
    {
        public static IConfigurationBuilder AddEswPsFile(this IConfigurationBuilder builder, string path)
        {
            return AddEswPsFile(builder, fileSystem: new FileSystem(), path: path);
        }

        public static IConfigurationBuilder AddEswPsFile(this IConfigurationBuilder builder, IFileSystem fileSystem, string path)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Invalid file path", nameof(path));
            }

            return builder.AddEswPsFile(s =>
            {
                s.Path = path;
                s.FileSystem = fileSystem;
            });
        }

        /// <summary>
        /// Adds a ESW PS file configuration source to <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IConfigurationBuilder"/> to add to.</param>
        /// <param name="configureSource">Configures the source.</param>
        /// <returns>The <see cref="IConfigurationBuilder"/>.</returns>
        public static IConfigurationBuilder AddEswPsFile(this IConfigurationBuilder builder, Action<EswPsFileConfigurationSource> configureSource)
            => builder.Add(configureSource);
    }
}
