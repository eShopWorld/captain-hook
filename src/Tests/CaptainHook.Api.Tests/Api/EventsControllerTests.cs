using System.Diagnostics;
using System.Threading.Tasks;
using CaptainHook.Api.Client.Models;
using CaptainHook.Api.Tests.Api;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using EShopworld.Security.Services.Testing.Settings;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class EventsControllerTests : ControllerTestBase
    {
        public EventsControllerTests(ApiClientFixture testFixture) : base(testFixture)
        {
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenUnauthenticated_Returns401Unauthorized() 
        {
            // Arrange
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await UnauthenticatedClient.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            CaptainHookContractSubscriberDto dto = GetTestSubscriberDto();
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName, dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created); 
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenUnauthenticated_Returns401Unauthorized() 
        {
            // Arrange
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await UnauthenticatedClient.PutWebhookWithHttpMessagesAsync(eventName, subscriberName);
            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenAuthenticated_Returns201Created() 
        {
            // Arrange
            CaptainHookContractEndpointDto dto = GetTestEndpointDto();
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await AuthenticatedClient.PutWebhookWithHttpMessagesAsync(eventName, subscriberName, dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        private string GetSubscriberName()
        {
            return "int-test-subscriber-" + Stopwatch.GetTimestamp(); // new every time
        }

        private string GetEventName()
        {
            return "integrationtest.event.type-" + Stopwatch.GetTimestamp();
        }

        private CaptainHookContractSubscriberDto GetTestSubscriberDto()
        {
            var webhooks = new CaptainHookContractWebhooksDto(endpoints: new[] { new CaptainHookContractEndpointDto()
            {
                Uri = "http://blah.blah",
                HttpVerb = "PUT",
                Authentication = GetTestAuthenticationDto()
            }
            });
            webhooks.SelectionRule = "$.tenantcode";
            return new CaptainHookContractSubscriberDto(webhooks);
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
