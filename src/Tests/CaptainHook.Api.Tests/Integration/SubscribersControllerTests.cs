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
    public class SubscribersControllerTests : ControllerTestBase
    {
        public SubscribersControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
        }

        [Fact, IsIntegration]
        public async Task GetAllSubscribers_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await UnauthenticatedClient.GetAllWithHttpMessagesAsync();

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task GetAllSubscribers_WhenAuthenticated_ReturnsNonEmptyList()
        {
            // Act 1
            var result = await AuthenticatedClient.GetAllWithHttpMessagesAsync();
            using (new AssertionScope())
            {
                // Assert 1
                result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
                
                // Act - Assert 2
                var content = await result.Response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        [Fact, IsIntegration]
        public async Task GetSubscriber_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await UnauthenticatedClient.GetSubscriberWithHttpMessagesAsync("core.events.test.trackingdomainevent;captain-hook");

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task GetSubscriber_WhenAuthenticated_Returns200AndData()
        {
            // Act
            var result = await AuthenticatedClient.GetSubscriberWithHttpMessagesAsync("core.events.test.trackingdomainevent;captain-hook");

            // Assert
            using (new AssertionScope())
            {
                result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
                var content = await result.Response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

    }
}
