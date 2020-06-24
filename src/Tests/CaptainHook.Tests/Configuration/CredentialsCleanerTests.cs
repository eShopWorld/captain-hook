using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class CredentialsCleanerTests
    {
        [Fact, IsLayer0]
        public void WhenSubscriberHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithOidcAuthentication()
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenSubscriberRouteHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithOidcAuthentication()))
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenCallbackHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.WithOidcAuthentication())
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].Callback.AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenCallbackRouteHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.AddWebhookRequestRule(
                    ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithOidcAuthentication())))
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].Callback.WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenSubscriberHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithBasicAuthentication()
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenSubscriberRouteHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithBasicAuthentication()))
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenCallbackHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.WithBasicAuthentication())
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].Callback.AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsLayer0]
        public void WhenCallbackRouteHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.AddWebhookRequestRule(
                    ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithBasicAuthentication())))
                .Create() };

            CredentialsCleaner.HideCredentials(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].Callback.WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }
    }
}