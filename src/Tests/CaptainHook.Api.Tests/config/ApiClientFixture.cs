using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Settings;
using EShopworld.Security.Services.Testing.Token;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        private readonly Uri _captainHookTestUri;

        public ApiClientFixture()
        {
            _captainHookTestUri = new Uri(EnvironmentSettings.Configuration["CaptainHookApiUri"]);
        }

        public ICaptainHookClient GetApiUnauthenticatedClient()
        {
            return new CaptainHookClient(_captainHookTestUri, AnonymousCredential.Instance);
        }

        public ICaptainHookClient GetApiClient()
        {
            var token = new TokenCredentialsBuilder().Build();
            return new CaptainHookClient(_captainHookTestUri, token);
        }

        private class AnonymousCredential : ServiceClientCredentials
        {
            public static ServiceClientCredentials Instance { get; } = new AnonymousCredential();

            private AnonymousCredential()
            {
            }
        }
    }
}