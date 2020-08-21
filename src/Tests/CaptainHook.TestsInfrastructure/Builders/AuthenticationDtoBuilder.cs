using System.Collections.Generic;
using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class AuthenticationDtoBuilder : SimpleBuilder<OidcAuthenticationDto>
    {
        public AuthenticationDtoBuilder()
        {
            With(x => x.Type, "OIDC");
            With(x => x.ClientId, "clientId");
            With(x => x.Uri, "https://security-api.com/token");
            With(x => x.Scopes, new List<string> { "test.scope.api" });
            With(x => x.ClientSecretKeyName, "secret-name");
        }
    }
}