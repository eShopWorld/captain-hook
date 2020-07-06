using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Tests.Builders;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class AuthenticationConfigSanitizerTests
    {
        [Fact, IsUnit]
        public void WhenSubscriberHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithOidcAuthentication()
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenSubscriberRouteHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithOidcAuthentication()))
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenCallbackHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.WithOidcAuthentication())
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].Callback.AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenCallbackRouteHasOidcAuth_ClientSecretShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.AddWebhookRequestRule(
                    ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithOidcAuthentication())))
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (OidcAuthenticationConfig)subscribers[0].Callback.WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.ClientSecret.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenSubscriberHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithBasicAuthentication()
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenSubscriberRouteHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .AddWebhookRequestRule(ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithBasicAuthentication()))
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenCallbackHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.WithBasicAuthentication())
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].Callback.AuthenticationConfig;
            auth.Password.Should().Be("***");
        }

        [Fact, IsUnit]
        public void WhenCallbackRouteHasBasicAuth_PasswordShouldBeMasked()
        {
            var subscribers = new[] { new SubscriberConfigurationBuilder()
                .WithCallback(callbackBuilder => callbackBuilder.AddWebhookRequestRule(
                    ruleBuilder => ruleBuilder.AddRoute(routeBuilder => routeBuilder.WithBasicAuthentication())))
                .Create() };

            AuthenticationConfigSanitizer.Sanitize(subscribers);

            var auth = (BasicAuthenticationConfig)subscribers[0].Callback.WebhookRequestRules[0].Routes[0].AuthenticationConfig;
            auth.Password.Should().Be("***");
        }
    }
}