using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Settings;
using EShopworld.Security.Services.Testing.Token;

namespace CaptainHook.Cli.Commands.ExecuteApi
{
    public class ApiClientFixture
    {
        private readonly Uri _captainHookTestUri;

        public ApiClientFixture()
        {
            _captainHookTestUri = new Uri(EnvironmentSettings.Configuration["CaptainHookApiUri"]);
        }

        public ICaptainHookClient GetApiClient()
        {
            var token = new TokenCredentialsBuilder().Build();
            return new CaptainHookClient(_captainHookTestUri, token);
        }
    }
}