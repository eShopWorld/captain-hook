using System.Diagnostics;
using System.Threading.Tasks;
using CaptainHook.Api.Client.Models;
using CaptainHook.Api.Tests.Config;
using EShopworld.Security.Services.Testing.Settings;
using Eshopworld.Tests.Core;
using FluentAssertions;
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
            var result = await UnauthenticatedClient.PutWebhookWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName);
            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            var dto = GetTestEndpointDto();

            // Act
            var result = await AuthenticatedClient.PutWebhookWithHttpMessagesAsync(IntegrationTestEventName, _subscriberName, dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        private CaptainHookContractSubscriberDto GetTestSubscriberDto()
        {
            var webhookDto = new CaptainHookContractWebhooksDto(endpoints: new[]
            {
                new CaptainHookContractEndpointDto()
                {
                    Uri = "http://blah.blah", HttpVerb = "PUT", Authentication = GetTestAuthenticationDto()
                }
            })
            {
                SelectionRule = "$.tenantcode"
            };
            return new CaptainHookContractSubscriberDto(webhookDto);
        }

        private CaptainHookContractAuthenticationDto GetTestAuthenticationDto()
        {
            return new CaptainHookContractAuthenticationDto(
                                "OIDC", "tooling.eda.client", EnvironmentSettings.StsSettings.Issuer,
                                new CaptainHookContractClientSecretDto(EnvironmentSettings.Configuration["KEYVAULT_URL"], "CaptainHook--ApiSecret"),
                                new string[] { "eda.peterpan.delivery.api.all" });
        }

        private CaptainHookContractEndpointDto GetTestEndpointDto()
        {
            return new CaptainHookContractEndpointDto("http://blah.blah", "PUT", GetTestAuthenticationDto());
        }
    }
}
