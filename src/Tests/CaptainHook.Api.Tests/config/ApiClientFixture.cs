using System;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Testing.Token;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        private static Uri CaptainHookTestUri = new Uri("https://localhost:24010");

        public ICaptainHookClient GetApiUnauthenticatedClient()
        {
            return new CaptainHookClient(CaptainHookTestUri, AnonymousCredential.Instance);
        }

        public ICaptainHookClient GetApiClient()
        {
            var token = new TokenCredentialsBuilder().Build();
            return new CaptainHookClient(CaptainHookTestUri, token);
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