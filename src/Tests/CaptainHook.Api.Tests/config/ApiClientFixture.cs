using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Settings;
using EShopworld.Security.Services.Testing.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        private Uri _CaptainHookTestUri;

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
            var tb = new RefreshingTokenProviderOptions(
                EnvironmentSettings.StsSettings.Issuer, 
                EnvironmentSettings.StsSettings.Subject, 
                EnvironmentSettings.StsSettings.ClientId, 
                EnvironmentSettings.StsSettings.Scopes, 
                EnvironmentSettings.StsSettings.Audience);
            var token = new TokenCredentialsBuilder(tb).Build();
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