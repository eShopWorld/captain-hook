using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Api.Tests.Config;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
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

        [Fact, IsDev]
        public async Task GetAllSubscribers_WhenUnauthenticated_ReturnsUnauthorized()
        {
            var result = await _unauthenticatedClient.GetAllWithHttpMessagesAsync();

            var content = await result.Response.Content.ReadAsStringAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact, IsDev]
        public async Task GetAllSubscribers_WhenAuthenticated_ReturnsNonEmptyList()
        {
            var result = await _authenticatedClient.GetAllWithHttpMessagesAsync();

            result.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
            var content = await result.Response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();
        }

        public void Dispose()
        {
            _authenticatedClient.Dispose();
            _unauthenticatedClient.Dispose();
        }
    }
}
