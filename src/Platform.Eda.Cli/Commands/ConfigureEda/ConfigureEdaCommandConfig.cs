using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    public sealed class ConfigureEdaCommandConfig
    {
        public Uri CaptainHookUrl { get; set; }

        public string ClientKeyVaultSecretName { get; set; }
    }
}