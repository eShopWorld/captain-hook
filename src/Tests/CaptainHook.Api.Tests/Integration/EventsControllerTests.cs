using System;
using System.Collections;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Client.Models;
using CaptainHook.Api.Tests.Config;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;
using Xunit.Abstractions;

namespace CaptainHook.Api.Tests.Integration
{
    public class EventsControllerTests : IClassFixture<EventFixture>
    {
        private readonly EventFixture _fixture;
        private readonly ITestOutputHelper _outputHelper;

        public EventsControllerTests(EventFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _outputHelper = outputHelper;

            _outputHelper.WriteLine("Environment variables:");
            var vars = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry entry in vars)
            {
                _outputHelper.WriteLine($"'{entry.Key}': '{entry.Value}'");
            }
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await _fixture.UnauthenticatedClient.PutSuscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventCreateName, _fixture.IntegrationTestSubscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutSubscriber_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            var dto = _fixture.GetTestSubscriberDto();

            // Act
            var result = await _fixture.AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventCreateName, _fixture.IntegrationTestSubscriberName, dto);

            // Assert
            await _outputHelper.PrintIfInvalidHttpResponse(result.Response);
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        [Fact, IsIntegration]
        public async Task UpdateSubscriber_WhenAuthenticated_Returns202Accepted()
        {
            // Arrange
            var uri = "https://www.modified.uri/path";
            var method = "POST";
            var dto = _fixture.GetTestSubscriberDto();
            dto.Webhooks.Endpoints[0].HttpVerb = method;
            dto.Webhooks.Endpoints[0].Uri = uri;

            // Act - Change in DTO and PUT again
            var result = await _fixture.AuthenticatedClient.PutSuscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventUpdateName, _fixture.IntegrationTestSubscriberName, dto);

            // Assert
            await _outputHelper.PrintIfInvalidHttpResponse(result.Response);
            result.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await _fixture.UnauthenticatedClient.PutWebhookWithHttpMessagesAsync(_fixture.IntegrationTestEventUpdateName, _fixture.IntegrationTestSubscriberName, "*");

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task PutWebhook_WhenAuthenticated_Returns201Created()
        {
            // Arrange
            var dto = _fixture.GetTestEndpointDto();

            // Act
            var result = await _fixture.AuthenticatedClient.PutWebhookWithHttpMessagesAsync(_fixture.IntegrationTestEventUpdateName, _fixture.IntegrationTestSubscriberName, "*", dto);

            // Assert
            await _outputHelper.PrintIfInvalidHttpResponse(result.Response);
            result.Response.StatusCode.Should().Be(StatusCodes.Status201Created);
        }

        [Fact, IsIntegration]
        public async Task DeleteEventSubscriber_WhenUnauthenticated_Returns401Unauthorized()
        {
            // Act
            var result = await _fixture.UnauthenticatedClient.DeleteSubscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventUpdateName, _fixture.IntegrationTestSubscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsIntegration]
        public async Task DeleteEventSubscriber_WhenAuthenticated_Returns200Ok()
        {
            // Arrange

            // Act
            var result = await _fixture.AuthenticatedClient.DeleteSubscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventDeleteName, _fixture.IntegrationTestSubscriberName);

            // Assert
            await _outputHelper.PrintIfInvalidHttpResponse(result.Response);
            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
        }

        [Fact, IsIntegration]
        public async Task DeleteEventSubscriber_WhenSubscriberNotExists_Returns404NotFound()
        {
            // Act
            var result = await _fixture.AuthenticatedClient.DeleteSubscriberWithHttpMessagesAsync(_fixture.IntegrationTestEventUpdateName, "unkwnown");

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }


        [Fact, IsIntegration]
        public async Task DeleteEventSubscriber_WhenEventNotExists_Returns404NotFound()
        {
            // Act
            var result = await _fixture.AuthenticatedClient.DeleteSubscriberWithHttpMessagesAsync("unkwnown", _fixture.IntegrationTestSubscriberName);

            // Assert
            result.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }


    }

    public class EventFixture : ApiClientFixture, IDisposable
    {
        public ICaptainHookClient AuthenticatedClient { get; }
        public ICaptainHookClient UnauthenticatedClient { get; }

        public string IntegrationTestEventCreateName => "captainhook.tests.integration.testevent.create";
        public string IntegrationTestEventUpdateName => "captainhook.tests.integration.testevent.update";
        public string IntegrationTestEventDeleteName => "captainhook.tests.integration.testevent.delete";
        public string IntegrationTestSubscriberName => "integration-tests-testsubscriber";

        public EventFixture()
        {
            AuthenticatedClient = this.GetApiClient();
            UnauthenticatedClient = this.GetApiUnauthenticatedClient();

            var dto = GetTestSubscriberDto();
            AuthenticatedClient
                .PutSuscriberWithHttpMessagesAsync(IntegrationTestEventUpdateName, IntegrationTestSubscriberName, dto)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            AuthenticatedClient
                .PutSuscriberWithHttpMessagesAsync(IntegrationTestEventDeleteName, IntegrationTestSubscriberName, dto)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public CaptainHookContractSubscriberDto GetTestSubscriberDto()
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

        public CaptainHookContractAuthenticationDto GetTestAuthenticationDto()
        {
            return new CaptainHookContractBasicAuthenticationDto("Basic", "user1", "sts--sts-secret--platform-captainhook-api-client");
        }

        public CaptainHookContractEndpointDto GetTestEndpointDto()
        {
            return new CaptainHookContractEndpointDto("http://blah.blah", "PUT", GetTestAuthenticationDto());
        }

        public void Dispose()
        {
            AuthenticatedClient
                .DeleteSubscriberWithHttpMessagesAsync(IntegrationTestEventCreateName, IntegrationTestSubscriberName)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            AuthenticatedClient
                .DeleteSubscriberWithHttpMessagesAsync(IntegrationTestEventUpdateName, IntegrationTestSubscriberName)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();

            AuthenticatedClient
                .DeleteSubscriberWithHttpMessagesAsync(IntegrationTestEventDeleteName, IntegrationTestSubscriberName)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
