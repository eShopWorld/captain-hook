using System;
using CaptainHook.Api.Client;
using CaptainHook.Api.Tests.Config;

namespace CaptainHook.Api.Tests.Integration
{
    public abstract class ControllerTestBase : IDisposable
    {

        protected readonly ICaptainHookClient AuthenticatedClient;
        protected readonly ICaptainHookClient UnauthenticatedClient;

        protected ControllerTestBase(ApiClientFixture testFixture)
        {
            AuthenticatedClient = testFixture.GetApiClient();
            UnauthenticatedClient = testFixture.GetApiUnauthenticatedClient();
        }

        public void Dispose()
        {
            AuthenticatedClient.Dispose();
            UnauthenticatedClient.Dispose();
        }
    }
}
