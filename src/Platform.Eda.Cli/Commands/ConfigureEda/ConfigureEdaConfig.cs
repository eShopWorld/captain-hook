using System;
using EShopworld.Security.Services.Rest;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class ConfigureEdaConfig
    {
        public Uri CaptainHookUrl { get; set; }

        public RefreshingTokenProviderOptions RefreshingTokenProviderOptions { get; set; }
    }
}