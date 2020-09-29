using CaptainHook.Common.Authentication;
using CaptainHook.Domain.Entities;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System.Linq;

namespace CaptainHook.Application.Tests
{
    public class AuthenticationConfigAssertions : ReferenceTypeAssertions<AuthenticationConfig, AuthenticationConfigAssertions>
    {
        public AuthenticationConfigAssertions(AuthenticationConfig instance)
        {
            Subject = instance;
        }

        protected override string Identifier => "AuthenticationConfig";

        public AndConstraint<AuthenticationConfigAssertions> BeValidConfiguration(AuthenticationConfig expectation, string because = "", params object[] becauseArgs)
        {
            Execute.Assertion
                .BecauseOf(because, becauseArgs)
                .Given(() => Subject)
                .ForCondition(authConfig => MatchesAuthentication(authConfig, expectation));

            return new AndConstraint<AuthenticationConfigAssertions>(this);
        }

        private static bool MatchesAuthentication(AuthenticationConfig config, AuthenticationConfig expectation)
        {
            return config.Type switch
            {
                AuthenticationType.Basic => config is BasicAuthenticationConfig basicConfig && MatchesBasicAuthentication(basicConfig, (BasicAuthenticationConfig)expectation),
                AuthenticationType.OIDC => config is OidcAuthenticationConfig oidcConfig && MatchesOidcAuthentication(oidcConfig, (OidcAuthenticationConfig)expectation),
                AuthenticationType.None => config is AuthenticationConfig noneConfig && noneConfig.Type == AuthenticationType.None,
                _ => false
            };
        }

        private static bool MatchesBasicAuthentication(BasicAuthenticationConfig config, BasicAuthenticationConfig expectation)
        {
            return
                config.Type == AuthenticationType.Basic &&
                config.Username == expectation.Username &&
                config.Password == expectation.Password;
        }

        private static bool MatchesOidcAuthentication(OidcAuthenticationConfig config, OidcAuthenticationConfig expectation)
        {
            return
                config.Type == AuthenticationType.OIDC &&
                config.Uri == expectation.Uri &&
                config.ClientId == expectation.ClientId &&
                config.ClientSecret == expectation.ClientSecret &&
                config.Scopes.SequenceEqual(expectation.Scopes);
        }
    }
}
