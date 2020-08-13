using CaptainHook.Api.Client;
using CaptainHook.Api.Tests.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Api.Tests.Api
{
    public abstract class ControllerTestBase : IDisposable
    {

        protected readonly ICaptainHookClient AuthenticatedClient;
        protected readonly ICaptainHookClient UnauthenticatedClient;

        public ControllerTestBase(ApiClientFixture testFixture)
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
