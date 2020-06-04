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
    public class RefreshConfigTests : IDisposable
    {
        private readonly ICaptainHookClient _apiClient;

        public RefreshConfigTests(ApiClientFixture testFixture)
        {
            _apiClient = testFixture.GetApiClient();
        }

        [Fact, IsDev]
        public async Task RefreshConfig_ValidEvent_ValidResponse()
        {
            var result = await _apiClient.RefreshConfigWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact, IsDev]
        public async Task RefreshConfig_NullEvent_InvalidResponse()
        {
            var result = await _apiClient.RefreshConfigWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        public void Dispose()
        {
            _apiClient.Dispose();
        }
    }
}
