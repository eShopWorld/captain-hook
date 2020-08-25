using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Xunit;
using IHttpClientFactory = CaptainHook.EventHandlerActor.Handlers.IHttpClientFactory;

namespace CaptainHook.Tests.Web.Authentication
{
    public class AuthenticationFactoryTests
    {
        private readonly IBigBrother _bigBrother;

        public AuthenticationFactoryTests()
        {
            _bigBrother = Mock.Of<IBigBrother>();
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

        public static IEnumerable<object[]> ChangeBasicAuthenticationTestData = new List<object[]>
            {
                new object[]
                {
                    new []
                    {
                        NewWebhookConfig("basic", "http://host1/api/v1/basic", "userblue", "initialPassword"),
                        NewWebhookConfig("basic", "http://host1/api/v1/basic", "userblue", "initialPassword")
                    }
                },
                new object[]
                {
                    new []
                    {
                        NewWebhookConfig("oidc", "http://localhost/api/v2/oidc", new OidcAuthenticationConfig()),
                        NewWebhookConfig("oidc", "http://localhost/api/v2/oidc", new OidcAuthenticationConfig()),
                    }
                }
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
        /// Checks that the auth handler changes with any change in basic auth params
        /// </summary>
        [Fact, IsUnit]
        public async Task When_BasicAuthParamsUpdated_ExpectUpdatedHandler()
        {
            // Arrange
            IEnumerable<WebhookConfig> changeBasicAuthenticationTestData = new List<WebhookConfig>
            {
                NewWebhookConfig("basic", "http://host1/api/v1/basic", "userblue", "initialPassword"),
                NewWebhookConfig("basic", "http://host1/api/v1/basic", "userblue", "changedPassword"),
                NewWebhookConfig("basic", "http://host1/api/v1/basic", "usergreen", "changedPassword"),
                NewWebhookConfig("basic", "http://host2/api/v1/basic", "usergreen", "differenturl")
            };

            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);
            var handlers = new List<IAuthenticationHandler>();

            // Act
            foreach (var webhookConfig in changeBasicAuthenticationTestData)
            {
                handlers.Add(await factory.GetAsync(webhookConfig, CancellationToken.None));
            }

            // Assert
            handlers.Should().OnlyHaveUniqueItems();
        }

        /// <summary>
        /// Checks that the auth factory returns the same handler does not change when auth params
        /// </summary>
        [Theory, IsUnit]
        [MemberData(nameof(ChangeBasicAuthenticationTestData))]
        public async Task When_AuthParamsUnchanged_ExpectSameHandler(WebhookConfig[] webhookConfigs)
        {
            // Arrange
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);
            var handlers = new List<IAuthenticationHandler>();
            
            // Act
            foreach (var webhookConfig in webhookConfigs)
            {
                handlers.Add(await factory.GetAsync(webhookConfig, CancellationToken.None));
            }

            // Assert
            handlers.Should().HaveCount(webhookConfigs.Length).
                And.AllBeEquivalentTo(handlers.FirstOrDefault(), "The Auth Config has not changed");
        }

        /// <summary>
        /// Checks that the auth handler changes with changes in OIDC auth parameters
        /// </summary>
        [Fact, IsUnit]
        public async Task When_OidcAuthParamsUpdated_ExpectUpdatedHandler()
        {
            // Arrange
            const string uri = "http://localhost/api/v2/oidc";
            var cancellationToken = new CancellationToken();

            var tokenWebhookConfigMap = GetOidcAuthChangeTestData(uri);
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), _bigBrother);

            // Act
            var handlers = new List<IAuthenticationHandler>();
            foreach (var webhookConfig in tokenWebhookConfigMap)
            {
                handlers.Add(await factory.GetAsync(webhookConfig, cancellationToken));
            }

            // Assert 
            handlers.Should().OnlyHaveUniqueItems();
        }

        private static List<WebhookConfig> GetOidcAuthChangeTestData(string uri)
        {
            return new List<WebhookConfig>
            {
                NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Start
                    "ClientId1", "secretv1", 200, uri, new[]{ "all" })),
                NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Change ClientId
                    "ClientId2", "secretv1", 20, uri, new[]{ "all" })) ,
                NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Change ClientSecret
                    "ClientId2", "secretv2", 20, uri, new[]{ "all" })) ,
                NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig( // Add Scope
                    "ClientId2", "secretv2", 20, uri, new[] { "all", "newScope", "removeScope" })) ,
                NewWebhookConfig("oidc", uri, NewOidcAuthenticationConfig(  // Remove Scope
                    "ClientId2", "secretv2", 20, uri, new[] { "all", "newScope" }))
            };
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

        private static WebhookConfig NewWebhookConfig(string name, string uri, string username, string password)
        {
            return NewWebhookConfig(name, uri, new BasicAuthenticationConfig { Type = AuthenticationType.Basic, Username = username, Password = password });
        }
    }
}
