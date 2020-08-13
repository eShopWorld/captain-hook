using System;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class RefreshConfigControllerTests : IDisposable
    {
        private readonly ICaptainHookClient _apiClient;

        public RefreshConfigControllerTests(ApiClientFixture testFixture)
        {
            _apiClient = testFixture.GetApiClient();
        }

        [Fact, IsDev]
        public async Task RefreshConfig_ValidEvent_ValidResponse()
        {
            var result = await _apiClient.ReloadConfigurationWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
