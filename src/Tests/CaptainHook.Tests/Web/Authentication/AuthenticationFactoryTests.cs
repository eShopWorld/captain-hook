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
        [IsLayer0]
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
        [IsLayer0]
        [Theory]
        [MemberData(nameof(NoneAuthenticationTestData))]
        public async Task NoAuthentication(WebhookConfig config)
        {
            var factory = new AuthenticationHandlerFactory(new HttpClientFactory(), new Mock<IBigBrother>().Object);

            var handler = await factory.GetAsync(config, CancellationToken.None);

            Assert.Null(handler);
        }
    }
}
