using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Telemetry;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using RichardSzalay.MockHttp;
using Xunit;
using IHttpClientFactory = CaptainHook.EventHandlerActor.Handlers.IHttpClientFactory;

namespace CaptainHook.Tests.Web.Authentication
{
    public class AuthenticationFactoryTests
    {
        private readonly BigBrother _bigBrother;

        public AuthenticationFactoryTests()
        {
            _bigBrother = new Mock<BigBrother>().Object;
        }

        public static IEnumerable<object[]> AuthenticationTestData =>
            new List<object[]>
            {
                new object[] { NewWebhookConfig("basic", "http://localhost/api/v1/basic", new BasicAuthenticationConfig()) , new BasicAuthenticationHandler(new BasicAuthenticationConfig()) },
                new object[] { NewWebhookConfig("oidc", "http://localhost/api/v2/oidc", new OidcAuthenticationConfig()), new OidcAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object) },
                new object[] { NewWebhookConfig("custom", "http://localhost/api/v3/custom", new OidcAuthenticationConfig { Type = AuthenticationType.Custom}), new MmAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object)  },

            };

        public static IEnumerable<object[]> NoneAuthenticationTestData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig { Name = "none", Uri = "http://localhost/api/v1/none"} }
            };

        public static IEnumerable<WebhookConfig> ChangeBasicAuthenticationTestData =>
            new List<WebhookConfig>
            {
                NewWebhookConfig("basic", "http://host1/api/v1/basic", NewBasicAuthenticationConfig("userblue", "initialPassword")),
                NewWebhookConfig("basic", "http://host1/api/v1/basic", NewBasicAuthenticationConfig("userblue", "changedPassword")),
                NewWebhookConfig("basic", "http://host1/api/v1/basic", NewBasicAuthenticationConfig("usergreen", "changedPassword")),
                NewWebhookConfig("basic", "http://host2/api/v1/basic", NewBasicAuthenticationConfig("usergreen", "differenturl"))
            };

        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="expectedHandler"></param>
        [IsUnit]
        [Theory]
        [MemberData(nameof(AuthenticationTestData))]
        public async Task AuthenticationHandlerFactory_WhenInvoked_ReturnsCorrectType(WebhookConfig config, IAuthenticationHandler expectedHandler)
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);

            var handler = await factory.GetAsync(config, CancellationToken.None);

            Assert.Equal(expectedHandler.GetType(), handler.GetType());
        }

        /// <summary>
        /// 
        /// </summary>
        [IsUnit]
        [Theory]
        [MemberData(nameof(NoneAuthenticationTestData))]
        public async Task When_AuthConfigIsNull_ExpectNullAuthenticationHandler(WebhookConfig config)
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);

            var handler = await factory.GetAsync(config, CancellationToken.None);

            Assert.Null(handler);
        }

        /// <summary>
        /// Checks that the auth token changes with any change in basic auth params
        /// </summary>
        [Fact, IsUnit]
        public async Task When_BasicAuthParamsUpdated_ExpectUpdatedTokenInRequests()
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);

            var expectedTokens = new List<string>();
            var receivedTokens = new List<string>();

            foreach (WebhookConfig webhookConfig in ChangeBasicAuthenticationTestData)
            {
                var authConfig = webhookConfig.AuthenticationConfig as BasicAuthenticationConfig;
                var handler = await factory.GetAsync(webhookConfig, CancellationToken.None);
                var token = await handler.GetTokenAsync(CancellationToken.None);

                expectedTokens.Add(
                    $"Basic {BasicAuthenticationHandler.CreateBasicAuthToken(authConfig.Username, authConfig.Password)}");
                receivedTokens.Add(token);
            }

            receivedTokens.Should().Equal(expectedTokens);
        }

        /// <summary>
        /// Checks that the auth token changes with changes in OIDC auth parameters
        /// </summary>
        [Fact, IsUnit]
        public async Task When_OidccAuthParamsUpdated_ExpectUpdatedTokenInRequests()
        {
            // Arrange
            var uri = "http://localhost/api/v2/oidc";
            var cancellationToken = new CancellationToken();

            var tokenWebhookConfigMap = GetOidcAuthChangeTestData(uri);
            var mockHttp = new MockHttpMessageHandler(BackendDefinitionBehavior.Always);
            
            foreach (var kvPair in tokenWebhookConfigMap)
            {
                var webhookConfig = kvPair.Value;
                var expectedAccessToken = kvPair.Key;

                SetupMockHttpResponse(mockHttp, webhookConfig.AuthenticationConfig as OidcAuthenticationConfig, expectedAccessToken);
            }

            /* Auth handler factory to respond using mock http client */
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(
                    new Dictionary<string, HttpClient> { { new Uri(uri).Host, mockHttp.ToHttpClient() } }), _bigBrother);

            // Act
            var receivedTokens = new List<string>();
            var expectedTokens = new List<string>();

            foreach (var kvPair in tokenWebhookConfigMap)
            {
                var webhookConfig = kvPair.Value;
                var expectedAccessToken = kvPair.Key;

                var handler = await factory.GetAsync(webhookConfig, cancellationToken);
                var token = await handler.GetTokenAsync(cancellationToken);

                expectedTokens.Add($"Bearer {expectedAccessToken}");
                receivedTokens.Add(token);
            }

            // Assert
            expectedTokens.Should().Equal(receivedTokens);
        }

        private static Dictionary<string, WebhookConfig> GetOidcAuthChangeTestData(string uri)
        {
            return new Dictionary<string, WebhookConfig>
            {
                { "token1", NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Start
                    "ClientId1", "secretv1", 200, uri, new[]{ "all" })) },
                { "token3", NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Change ClientId
                    "ClientId2", "secretv1", 20, uri, new[]{ "all" })) },
                { "token4", NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Change ClientSecret
                    "ClientId2", "secretv2", 20, uri, new[]{ "all" })) },
                { "token5", NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Add Scope
                    "ClientId2", "secretv2", 20, uri, new[] { "all", "newScope", "removeScope" })) },
                { "token6", NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig(  // Remove Scope
                    "ClientId2", "secretv2", 20, uri, new[] { "all", "newScope" })) }
            };
        }

        private static void SetupMockHttpResponse(MockHttpMessageHandler mockHttp, OidcAuthenticationConfig config, string expectedAccessToken)
        {
            mockHttp.When(HttpMethod.Post, config.Uri)
                .WithFormData("client_id", config.ClientId)
                .WithFormData("client_secret", config.ClientSecret)
                .WithFormData("scope", string.Join(" ", config.Scopes))
                .Respond(HttpStatusCode.OK, "application/json",
                    JsonConvert.SerializeObject(new OidcAuthenticationToken
                    {
                        AccessToken = expectedAccessToken
                    }));

        }

        private static OidcAuthenticationConfig NewOidcAuthenticationConfig(string clientId, string clientSecret, int refreshBeforeInSeconds, string uri, string[] scopes)
        {
            return new OidcAuthenticationConfig
            {
                ClientId = clientId,
                ClientSecret = clientSecret,
                RefreshBeforeInSeconds = refreshBeforeInSeconds,
                Type = AuthenticationType.OIDC,
                Uri = uri,
                Scopes = scopes
            };
        }

        private static WebhookConfig NewWebhookConfig(string name, string uri, AuthenticationConfig config)
        {
            return new WebhookConfig
            { Name = name, Uri = uri, AuthenticationConfig = config };
        }

        private static AuthenticationConfig NewBasicAuthenticationConfig(string username, string password)
        {
            return new BasicAuthenticationConfig
            { Type = AuthenticationType.Basic, Username = username, Password = password };
        }
    }
}
