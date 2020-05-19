using System;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class ProbeTests : IDisposable
    {
        private readonly ICaptainHookClient _unauthenticatedClient;

        public ProbeTests(ApiClientFixture testFixture)
        {
            _unauthenticatedClient = testFixture.GetApiUnauthenticatedClient();
        }

        [Fact, IsDev]
        public async Task GetProbe_QueryProbe_ValidResponse()
        {
            var response = await _unauthenticatedClient.GetProbeWithHttpMessagesAsync();
            var content = await response.Response.Content.ReadAsStringAsync();

            using (new AssertionScope())
            {
                response.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
                content.Should().Be("Healthy");
            }
        }

        public void Dispose()
        {
            _unauthenticatedClient.Dispose();
        }
    }
}
