using System.Diagnostics;
using System.Threading.Tasks;
using CaptainHook.Api.Client.Models;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests.Integration
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class EventsControllerTests : ControllerTestBase
    {
        private readonly string _subscriberName;
        private const string IntegrationTestEventName = "captainhook.tests.web.integration.testevent";

        public EventsControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
            _subscriberName = "int-test-subscriber-" + Stopwatch.GetTimestamp();
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await UnauthenticatedClient.PutSuscriberWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            var dto = GetTestSubscriberDto();

            // Act
            var result = await AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, dto);

            // Assert

            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        [Fact, IsIntegration]
        public async Task UpdateSubscriber_WhenAuthenticated_Returns202Accepted()
        {
            // Arrange
            var dto = GetTestSubscriberDto();
            await AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, dto);

            // Act - Change in DTO and PUT again
            dto.Webhooks.Endpoints[0].HttpVerb = "POST";
            var result = await AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await UnauthenticatedClient.PutWebhookWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, "*");
            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            var dto = GetTestEndpointDto();

            // Act
            var result = await AuthenticatedClient.PutWebhookWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, "*", dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        [Fact, IsIntegration]
        public async Task GetSubscriber_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await UnauthenticatedClient.GetAllWithHttpMessagesAsync();

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        public async Task GetSubscriber_WhenAuthenticated_Returns200AndData()
        {
            // Act
            var result = await AuthenticatedClient.GetSubscriberWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName);

            // Assert
            using (new AssertionScope())
            {
                result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
                var content = await result.Response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();
            }
        }

        private CaptainHookContractSubscriberDto GetTestSubscriberDto()
        {
            var webhookDto = new CaptainHookContractWebhooksDto(endpoints: new[]
            {
                new CaptainHookContractEndpointDto()
                {
                    Uri = "http://blah.blah", HttpVerb = "PUT", Authentication = GetTestAuthenticationDto(), Selector = "*"
                }
            })
            {
                SelectionRule = "$.tenantcode"
            };
            return new CaptainHookContractSubscriberDto(webhookDto);
        }

        private static CaptainHookContractAuthenticationDto GetTestAuthenticationDto()
        {
            return new CaptainHookContractBasicAuthenticationDto("Basic", "user1", "pass1");
        }

        private static CaptainHookContractEndpointDto GetTestEndpointDto()
        {
            return new CaptainHookContractEndpointDto("http://blah.blah", "PUT", GetTestAuthenticationDto());
        }
    }
}
