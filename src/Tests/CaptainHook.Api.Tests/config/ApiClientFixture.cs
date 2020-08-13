using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Settings;
using EShopworld.Security.Services.Testing.Token;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        private readonly Uri _CaptainHookTestUri;

        public ApiClientFixture()
        {
            _CaptainHookTestUri = new Uri(EnvironmentSettings.Configuration["CaptainHookApiUri"]);
        }

        public ICaptainHookClient GetApiUnauthenticatedClient()
        {
            return new CaptainHookClient(_CaptainHookTestUri, AnonymousCredential.Instance);
        }

        public ICaptainHookClient GetApiClient()
        {
            var token = new TokenCredentialsBuilder().Build();
            return new CaptainHookClient(_CaptainHookTestUri, token);
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