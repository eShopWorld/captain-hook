using CaptainHook.Common;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using Eshopworld.Core;
using Eshopworld.Tests.Core;
using Moq;
using Xunit;

namespace CaptainHook.UnitTests.Authentication
{
    public class AuthenticationFactoryTests
    {
        [IsLayer0]
        [Theory]
        [InlineData("none", null)]
        [InlineData("basic", typeof(BasicAcquireTokenHandler))]
        [InlineData("oauth", typeof(OAuthTokenHandler))]
        [InlineData("custom", typeof(MmOAuthAuthenticationHandler))]
        public void GetTokenProvider(string authenticationType, object instance)
        {
            var indexedDictionary = new IndexDictionary<string, WebhookConfig>
            {
                {
                    "none", new WebhookConfig
                    {
                        AuthenticationType = AuthenticationType.None,
                        Name = "hello1",
                    }
                },
                {
                    "basic", new WebhookConfig
                    {
                        AuthenticationType = AuthenticationType.Basic,
                        Name = "hello2",
                        AuthenticationConfig = new BasicAuthenticationConfig()
                    }
                },
                {
                    "oauth", new WebhookConfig
                    {
                        AuthenticationType = AuthenticationType.OAuth,
                        Name = "hello3",
                        AuthenticationConfig = new OAuthAuthenticationConfig()
                    }
                },
                {
                    "custom", new WebhookConfig
                    {
                        AuthenticationType = AuthenticationType.Custom,
                        Name = "hello4",
                        AuthenticationConfig = new OAuthAuthenticationConfig()
                    }
                }
            };

            var factory = new AuthenticationHandlerFactory(indexedDictionary, new Mock<IBigBrother>().Object);

            var handler = factory.Get(authenticationType);

            Assert.Equal(instance.GetType(), handler.GetType());
        }
    }
}
