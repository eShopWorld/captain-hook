using System;
using System.Diagnostics.CodeAnalysis;
using EShopworld.Security.Services.Rest;
using JetBrains.Annotations;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [UsedImplicitly]
    [ExcludeFromCodeCoverage]
    public sealed class ConfigureEdaCommandConfig
    {
        public Uri CaptainHookUrl { get; set; }

        public RefreshingTokenProviderOptions RefreshingTokenProviderOptions { get; set; }
    }
}