using System.Collections;
using System.Collections.Generic;
using CaptainHook.Common.Authentication;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;

namespace CaptainHook.Application.Tests.Infrastructure.SubscriberEntityToConfigurationMapperTests
{
    public class MapSubscriberAsyncTestData : IEnumerable<object[]>
    {
        public static readonly string Secret = "MySecret";

        private static readonly OidcAuthenticationEntity OidcAuthenticationEntity =
            new OidcAuthenticationEntity("captain-hook-id", "kv-secret-name", "https://blah-blah.sts.eshopworld.com", new[] { "scope1" });

        private static readonly BasicAuthenticationEntity BasicAuthenticationEntity =
            new BasicAuthenticationEntity("mark", "kv-secret-name");

        private static readonly OidcAuthenticationConfig OidcAuthenticationConfig =
            new OidcAuthenticationConfig
            {
                ClientId = "captain-hook-id",
                ClientSecret = Secret,
                Scopes = new[] { "scope1" },
                Uri = "https://blah-blah.sts.eshopworld.com",
                Type = AuthenticationType.OIDC
            };

        private static readonly BasicAuthenticationConfig BasicAuthenticationConfig =
            new BasicAuthenticationConfig { Username = "mark", Password = Secret, Type = AuthenticationType.Basic };

        private static readonly AuthenticationConfig NoAuthenticationConfig = new AuthenticationConfig();

        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[] { "POST", OidcAuthenticationEntity, OidcAuthenticationConfig };
            yield return new object[] { "GET", OidcAuthenticationEntity, OidcAuthenticationConfig };
            yield return new object[] { "PUT", OidcAuthenticationEntity, OidcAuthenticationConfig };
            yield return new object[] { "POST", BasicAuthenticationEntity, BasicAuthenticationConfig };
            yield return new object[] { "GET", BasicAuthenticationEntity, BasicAuthenticationConfig };
            yield return new object[] { "PUT", BasicAuthenticationEntity, BasicAuthenticationConfig };
            yield return new object[] { "POST", null, NoAuthenticationConfig };
            yield return new object[] { "GET", null, NoAuthenticationConfig };
            yield return new object[] { "PUT", null, NoAuthenticationConfig };
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}