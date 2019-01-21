﻿using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Tests.Core;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class MmAuthenticationHandlerTests
    {
        [Theory]
        [InlineData("6015CF7142BA060F5026BE9CC442C12ED7F0D5AECCBAA0678DEEBC51C6A1B282")]
        [IsLayer1]
        public async Task AuthorisationTokenTests(string expectedAccessToken)
        {
            var expectedResponse = JsonConvert.SerializeObject(new AuthToken
            {
                AccessToken = expectedAccessToken
            });

            var config = new AuthenticationConfig
            {
                ClientId = "bob",
                ClientSecret = "bobsecret",
                Uri = "http://localhost/authendpoint"
            };

            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(HttpMethod.Post, config.Uri)
                .WithHeaders("client_id", config.ClientId)
                .WithHeaders("client_secret", config.ClientSecret)
                .WithContentType("application/json-patch+json", string.Empty)
                .Respond(HttpStatusCode.Created, "application/json-patch+json", expectedResponse);

            var handler = new MmAuthenticationHandler(config);
            var httpClient = mockHttp.ToHttpClient();
            await handler.GetToken(httpClient);

            Assert.NotNull(httpClient.DefaultRequestHeaders.Authorization);
            Assert.Equal(expectedAccessToken, httpClient.DefaultRequestHeaders.Authorization.Parameter);
            Assert.Equal("Bearer", httpClient.DefaultRequestHeaders.Authorization.Scheme);
        }
    }
}
