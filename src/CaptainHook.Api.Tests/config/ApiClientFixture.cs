using System;
using CaptainHook.Api.Client;
using Microsoft.Rest;

namespace CaptainHook.Api.Tests.Config
{
    public class ApiClientFixture
    {
        public ICaptainHookClient GetApiUnauthenticatedClient()
        {
            var url = new Uri("https://localhost:24010");

            return new CaptainHookClient(url, AnonymousCredential.Instance);
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