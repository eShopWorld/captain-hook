﻿using System.Collections.Generic;
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
                new object[] { "basic", new BasicAuthenticationConfig(), new BasicAuthenticationHandler(new BasicAuthenticationConfig()),  },
                new object[] { "oidc", new OidcAuthenticationConfig(), new OidcAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object) },
                new object[] { "custom", new OidcAuthenticationConfig{ Type = AuthenticationType.Custom}, new MmAuthenticationHandler(new Mock<IHttpClientFactory>().Object, new OidcAuthenticationConfig(), new Mock<IBigBrother>().Object)  }
            };

        public static IEnumerable<object[]> NoneAuthenticationTestData =>
            new List<object[]>
            {
                new object[] {"none", new AuthenticationConfig()}
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationName"></param>
        /// <param name="authenticationConfig"></param>
        /// <param name="expectedHandler"></param>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(AuthenticationTestData))]
        public async Task GetTokenProvider(string configurationName, AuthenticationConfig authenticationConfig, IAcquireTokenHandler expectedHandler)
        {
            var indexedDictionary = new IndexDictionary<string, WebhookConfig>
            {
                {
                    configurationName, new WebhookConfig
                    {
                        Name = configurationName,
                        AuthenticationConfig = authenticationConfig
                    }
                }
            };

            var factory = new AuthenticationHandlerFactory(indexedDictionary, new Mock<IBigBrother>().Object, new Mock<IHttpClientFactory>().Object);

            var handler = await factory.GetAsync(configurationName, CancellationToken.None);

            Assert.Equal(expectedHandler.GetType(), handler.GetType());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationName"></param>
        /// <param name="authenticationConfig"></param>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(NoneAuthenticationTestData))]
        public async Task NoAuthentication(string configurationName, AuthenticationConfig authenticationConfig)
        {
            var indexedDictionary = new IndexDictionary<string, WebhookConfig>
            {
                {
                    configurationName, new WebhookConfig
                    {
                        Name = configurationName,
                        AuthenticationConfig = authenticationConfig
                    }
                }
            };

            var factory = new AuthenticationHandlerFactory(indexedDictionary, new Mock<IBigBrother>().Object, new Mock<IHttpClientFactory>().Object);

            var handler = await factory.GetAsync(configurationName, CancellationToken.None);

            Assert.Null(handler);
        }
    }
}
