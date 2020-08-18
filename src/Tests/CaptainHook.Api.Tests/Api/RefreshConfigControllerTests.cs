using System.Threading.Tasks;
using CaptainHook.Api.Tests.Api;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class RefreshConfigControllerTests : ControllerTestBase
    {
        public RefreshConfigControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
        }


        [Fact(Skip = "Skipped: a successful RefreshConfig makes the other tests fail until refresh is complete."), IsIntegration]
        public async Task RefreshConfig_WhenAuthenticated_Returns202Accepted()
        {
            var result = await AuthenticatedClient.ReloadConfigurationWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        [Fact, IsIntegration]
        public async Task RefreshConfig_WhenUnauthenticated_Returns401Unauthorized()
        {
            var result = await UnauthenticatedClient.ReloadConfigurationWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}
