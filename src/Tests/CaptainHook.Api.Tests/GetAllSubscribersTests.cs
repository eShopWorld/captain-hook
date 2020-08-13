using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using EShopworld.Security.Services.Testing.Settings;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace CaptainHook.Api.Tests
{
    [Collection(ApiClientCollection.TestFixtureName)]
    public class GetAllSubscribersTests : IDisposable
    {
        private readonly ICaptainHookClient _authenticatedClient;
        private readonly ICaptainHookClient _unauthenticatedClient;

        public GetAllSubscribersTests(ApiClientFixture testFixture)
        {
            _authenticatedClient = testFixture.GetApiClient();
            _unauthenticatedClient = testFixture.GetApiUnauthenticatedClient();
        }

        [Fact, IsIntegration]
        public async Task GetAllSubscribers_WhenUnauthenticated_ReturnsUnauthorized()
        {
            // Act
            var result = await _unauthenticatedClient.GetAllWithHttpMessagesAsync();

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task GetAllSubscribers_WhenAuthenticated_ReturnsNonEmptyList()
        {
            // Act - Assert 1
            var result = await _authenticatedClient.GetAllWithHttpMessagesAsync();
            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);

            // Act - Assert 2
            var content = await result.Response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenUnauthenticated_Return401Unauthorized() 
        {
            // Arrange
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await _unauthenticatedClient.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenAuthenticated_ShouldCreateResource()
        {
            // Arrange
            CaptainHookContractSubscriberDto dto = GetTestSubscriberDto();
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await _authenticatedClient.PutSuscriberWithHttpMessagesAsync(eventName, subscriberName, dto);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created); 
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenUnauthenticated_Return401Unauthorized() 
        {
            // Arrange
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await _unauthenticatedClient.PutWebhookWithHttpMessagesAsync(eventName, subscriberName);
            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenAuthenticated_ShouldCreateResource() 
        {
            // Arrange
            CaptainHookContractEndpointDto dto = GetTestEndpointDto();
            string eventName = GetEventName();
            string subscriberName = GetSubscriberName();

            // Act
            var result = await _authenticatedClient.PutWebhookWithHttpMessagesAsync(eventName, subscriberName, dto);

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

        public void Dispose()
        {
            _authenticatedClient.Dispose();
            _unauthenticatedClient.Dispose();
        }
    }
}
