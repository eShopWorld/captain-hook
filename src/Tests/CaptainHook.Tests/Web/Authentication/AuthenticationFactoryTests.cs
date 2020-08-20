using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Web.Authentication
{
    public class AuthenticationFactoryTests
    {
        public static IEnumerable<object[]> AuthenticationTestData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig{Name = "basic", Uri = "http://localhost/api/v1/basic", AuthenticationConfig = new BasicAuthenticationConfig()}, new BasicAuthenticationHandler(new BasicAuthenticationConfig()),  },
                new object[] { new WebhookConfig{Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig()}, new OidcAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object) },
                new object[] { new WebhookConfig{Name = "custom", Uri = "http://localhost/api/v3/custom", AuthenticationConfig = new OidcAuthenticationConfig{ Type = AuthenticationType.Custom}}, new MmAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object)  },

            };

        public static IEnumerable<object[]> AuthenticationChangeTestData => new List<object[]>
        {
            new object[] {
                new List<WebhookConfig>
                {
                    new WebhookConfig { Name = "basic", Uri = "http://host1/api/v1/basic", AuthenticationConfig = new BasicAuthenticationConfig { Type = AuthenticationType.Basic, Username = "userblue", Password = "initialPassword" }},
                    new WebhookConfig { Name = "basic", Uri = "http://host1/api/v1/basic", AuthenticationConfig = new BasicAuthenticationConfig { Type = AuthenticationType.Basic, Username = "userblue", Password = "changedPassword" }},
                    new WebhookConfig { Name = "basic", Uri = "http://host1/api/v1/basic", AuthenticationConfig = new BasicAuthenticationConfig { Type = AuthenticationType.Basic, Username = "usergreen", Password = "changedPassword" }},
                    new WebhookConfig { Name = "basic", Uri = "http://host2/api/v1/basic", AuthenticationConfig = new BasicAuthenticationConfig { Type = AuthenticationType.Basic, Username = "usergreen", Password = "differenturl" }}
                }
            },
            new object[] {
                new List<WebhookConfig>
            {
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Start
                        ClientId = "ClientId1", ClientSecret = "secretv1", RefreshBeforeInSeconds = 200, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new []{ "all" }
                    }
                },
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Change RefreshInterval
                        ClientId = "ClientId1", ClientSecret = "secretv1", RefreshBeforeInSeconds = 20, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new []{ "all" }
                    }
                },
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Change ClientId
                        ClientId = "ClientId2", ClientSecret = "secretv1", RefreshBeforeInSeconds = 20, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new []{ "all" }
                    }
                },
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Change ClientSecret
                        ClientId = "ClientId2", ClientSecret = "secretv2", RefreshBeforeInSeconds = 20, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new []{ "all" }
                    }
                },
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Add Scope
                        ClientId = "ClientId2", ClientSecret = "secretv2", RefreshBeforeInSeconds = 20, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new [] { "all", "newScope", "removeScope" }
                    }
                },
                new WebhookConfig { Name = "oidc", Uri = "http://localhost/api/v2/oidc", AuthenticationConfig = new OidcAuthenticationConfig
                    { // Remove Scope
                        ClientId = "ClientId2", ClientSecret = "secretv2", RefreshBeforeInSeconds = 20, Type = AuthenticationType.OIDC, Uri = "http://localhost/api/v2/oidc", Scopes = new [] { "all", "newScope" }
                    }
                }
            }

            }
        };

        public static IEnumerable<object[]> NoneAuthenticationTestData =>
            new List<object[]>
            {
                new object[] { new WebhookConfig { Name = "none", Uri = "http://localhost/api/v1/none"} }
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="config"></param>
        /// <param name="expectedHandler"></param>
        [IsUnit]
        [Theory]
        [MemberData(nameof(AuthenticationTestData))]
        public async Task GetTokenProvider(WebhookConfig config, IAuthenticationHandler expectedHandler)
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), new Mock<IBigBrother>().Object);

            var handler = await factory.GetAsync(config, CancellationToken.None);

            Assert.Equal(expectedHandler.GetType(), handler.GetType());
        }


        /// <summary>
        /// 
        /// </summary>
        [IsUnit]
        [Theory]
        [MemberData(nameof(NoneAuthenticationTestData))]
        public async Task NoAuthentication(WebhookConfig config)
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), new Mock<IBigBrother>().Object);

            var handler = await factory.GetAsync(config, CancellationToken.None);

            Assert.Null(handler);
        }

        [IsUnit]
        [Theory]
        [MemberData(nameof(AuthenticationChangeTestData))]
        public async Task ChangeAuthentication(IEnumerable<WebhookConfig> flippingAuthenticationTestData)
        {


            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), new Mock<IBigBrother>().Object);
            string lastToken = null;
            foreach (WebhookConfig webhookConfig in flippingAuthenticationTestData)
            {
                var handler = await factory.GetAsync(webhookConfig, CancellationToken.None);
                var token = await handler.GetTokenAsync(CancellationToken.None);
                Assert.NotEqual(token, lastToken);
                lastToken = token;
            }
        }
    }
}
