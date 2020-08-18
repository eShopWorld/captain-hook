using System;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Rest;
using Polly;
using Xunit;

namespace CaptainHook.Api.Tests.Integration
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class RefreshConfigControllerTests : ControllerTestBase
    {
        public RefreshConfigControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
        }

        [Fact, IsIntegration]
        public async Task RefreshConfig_WhenAuthenticated_Returns202AcceptedAndWaitForReloadToFinish()
        {
            var configStatusRetryPolicy = Policy /* poll until no conflict */
                .HandleResult<HttpOperationResponse>(msg => msg.Response.StatusCode == HttpStatusCode.Accepted)
                .WaitAndRetryAsync(100, i => TimeSpan.FromSeconds(2d));

            // Act 1
            var result = await AuthenticatedClient.ReloadConfigurationWithHttpMessagesAsync();
            // Assert - Reload should return 202
            result.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);

            // Act 2
            result = await AuthenticatedClient.ReloadConfigurationWithHttpMessagesAsync();
            // Assert - Subsequent immediate reload should return 409
            result.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);

            // 3 - Wait until the reload status is green (Ok)
            result = await configStatusRetryPolicy.ExecuteAsync(async () =>
                await AuthenticatedClient.GetConfigurationStatusWithHttpMessagesAsync());

            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact, IsIntegration]
        public async Task GetConfigurationStatus_WhenUnauthenticated_Returns401Unauthorized()
        {
            var result = await UnauthenticatedClient.GetConfigurationStatusWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task GetConfigurationStatus_WhenAuthenticated_Returns200Ok()
        {
            var result = await AuthenticatedClient.GetConfigurationStatusWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact, IsIntegration]
        public async Task RefreshConfig_WhenUnauthenticated_Returns401Unauthorized()
        {
            var result = await UnauthenticatedClient.ReloadConfigurationWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}
