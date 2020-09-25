using System;
using JetBrains.Annotations;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [UsedImplicitly]
    public sealed class ConfigureEdaCommandConfig
    {
        public Uri CaptainHookUrl { get; set; }

        public string ClientKeyVaultSecretName { get; set; }
    }
}