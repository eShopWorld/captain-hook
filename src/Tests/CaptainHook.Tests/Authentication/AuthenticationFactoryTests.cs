using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.Tests.Authentication
{
    public class AuthenticationFactoryTests
    {
        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "basic", new BasicAuthenticationConfig(), AuthenticationType.Basic, new BasicTokenHandler(new BasicAuthenticationConfig()),  },
                new object[] { "oauth", new OAuthAuthenticationConfig(), AuthenticationType.OAuth, new OAuthTokenHandler(new OAuthAuthenticationConfig()) },
                new object[] { "custom", new OAuthAuthenticationConfig(), AuthenticationType.Custom, new MmOAuthAuthenticationHandler(new OAuthAuthenticationConfig())  }
            };

        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationName"></param>
        /// <param name="authenticationConfig"></param>
        /// <param name="authenticationType"></param>
        /// <param name="expectedHandler"></param>
        [IsLayer0]
        [Theory]
        [MemberData(nameof(Data))]
        public void GetTokenProvider(string configurationName, AuthenticationConfig authenticationConfig, AuthenticationType authenticationType, IAcquireTokenHandler expectedHandler)
        {
            var indexedDictionary = new IndexDictionary<string, WebhookConfig>
            {
                {
                    configurationName, new WebhookConfig
                    {
                        AuthenticationType = authenticationType,
                        Name = configurationName,
                        AuthenticationConfig = authenticationConfig
                    }
                }
            };

            var factory = new AuthenticationHandlerFactory(indexedDictionary, new Mock<IBigBrother>().Object);

            var handler = factory.Get(configurationName);

            Assert.Equal(expectedHandler.GetType(), handler.GetType());
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="configurationName"></param>
        /// <param name="authenticationConfig"></param>
        /// <param name="authenticationType"></param>
        [IsLayer0]
        [Theory]
        [InlineData("none", null, AuthenticationType.None)]
        public void NoAuthentication(string configurationName, AuthenticationConfig authenticationConfig, AuthenticationType authenticationType)
        {
            var indexedDictionary = new IndexDictionary<string, WebhookConfig>
            {
                {
                    configurationName, new WebhookConfig
                    {
                        AuthenticationType = authenticationType,
                        Name = configurationName,
                        AuthenticationConfig = authenticationConfig
                    }
                }
            };

            var factory = new AuthenticationHandlerFactory(indexedDictionary, new Mock<IBigBrother>().Object);

            var handler = factory.Get(configurationName);

            Assert.Null(handler);
        }
    }
}
