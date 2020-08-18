using System.Threading.Tasks;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests.Integration
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class ProbeControllerTests : ControllerTestBase
    {
        public ProbeControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
        }

        [Fact, IsDev]
        public async Task GetProbe_QueryProbe_ValidResponse()
        {
            var response = await UnauthenticatedClient.GetProbeWithHttpMessagesAsync();
            var content = await response.Response.Content.ReadAsStringAsync();

            using (new AssertionScope())
            {
                response.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
                content.Should().Be("Healthy");
            }
        }
    }
}
