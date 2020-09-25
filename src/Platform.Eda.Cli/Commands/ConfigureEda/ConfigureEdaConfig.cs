using System;
using System.Diagnostics.CodeAnalysis;
using EShopworld.Security.Services.Rest;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    [ExcludeFromCodeCoverage]
    public class ConfigureEdaConfig
    {
        public Uri CaptainHookUrl { get; set; }

        public RefreshingTokenProviderOptions RefreshingTokenProviderOptions { get; set; }
    }
}