using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using Eshopworld.Tests.Core;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Collections.Generic;
using Xunit;

namespace CaptainHook.Tests.Configuration
{
    public class SubscriberConfigurationTests
    {
        private static readonly OidcAuthenticationConfig OidcAuthenticationConfig = new OidcAuthenticationConfig { ClientId = "captain-hook-id", ClientSecret = "Secret", Scopes = new[] { "scope1" }, Uri = "https://blah-blah.sts.eshopworld.com", Type = AuthenticationType.OIDC };
        private static readonly BasicAuthenticationConfig BasicAuthenticationConfig = new BasicAuthenticationConfig { Username = "mark", Password = "Secret", Type = AuthenticationType.Basic };

        public static IEnumerable<object[]> Data =>
            new List<object[]>
            {
                new object[] { "POST", OidcAuthenticationConfig },
                new object[] { "GET", OidcAuthenticationConfig },
                new object[] { "PUT", OidcAuthenticationConfig },
                new object[] { "POST", BasicAuthenticationConfig },
                new object[] { "GET", BasicAuthenticationConfig },
                new object[] { "PUT", BasicAuthenticationConfig }
            };

        [Theory, IsUnit]
        [MemberData(nameof(Data))]
        public void When_WebhookConfigIsMapped_Then_ValidSubscriberConfigurationIsReturned(string httpVerb, AuthenticationConfig authenticationConfig)
        {
            var name = "sample-name";
            var eventType = "sample-event-type";
            var uri = "http://www.uri.com/sample/path";
            var requestRules = new List<WebhookRequestRule>()
            {
                new WebhookRequestRule()
                {
                    Source = new SourceParserLocation
                    {
                        RuleAction = RuleAction.RouteAndReplace,
                        Replace = new Dictionary<string, string> { { "orderNumber", "$.OrderNumber" } }
                    },
                    Destination = new ParserLocation
                    {
                        Path = "{orderNumber}"
                    },
                    Routes = new List<WebhookConfigRoute>
                    {
                        new WebhookConfigRoute
                        {
                            AuthenticationConfig = authenticationConfig,
                            Selector = "selector1",
                            HttpVerb = httpVerb,
                            EventType = eventType,
                            Name = name,
                            Uri = "https://www.uri2.com/path"
                        },
                        new WebhookConfigRoute
                        {
                            AuthenticationConfig = authenticationConfig,
                            Selector = "selector2",
                            HttpVerb = httpVerb,
                            EventType = eventType,
                            Name = name,
                            Uri = "https://www.uri3.com/path"
                        }
                    }

                }
            };

            var webhookConfig = new WebhookConfig()
            {
                Name = name,
                EventType = eventType,
                Uri = uri,
                HttpVerb = httpVerb,
                AuthenticationConfig = authenticationConfig,
                WebhookRequestRules = requestRules
            };

            var result = SubscriberConfiguration.FromWebhookConfig(webhookConfig);

            using(new AssertionScope())
            result.Name.Should().Be(name);
            result.EventType.Should().Be(eventType);
            result.Uri.Should().Be(uri);
            result.HttpVerb.Should().Be(httpVerb);
            result.AuthenticationConfig.Should().Be(authenticationConfig);
            result.WebhookRequestRules.Should().BeEquivalentTo(requestRules);
        }
    }
}
