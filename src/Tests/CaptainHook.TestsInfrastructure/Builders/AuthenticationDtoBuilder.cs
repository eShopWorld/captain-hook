using System.Collections.Generic;
using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class AuthenticationDtoBuilder : SimpleBuilder<AuthenticationDto>
    {
        public AuthenticationDtoBuilder()
        {
            With(x => x.Type, "OIDC");
            With(x => x.ClientId, "clientId");
            With(x => x.Uri, "https://security-api.com/token");
            With(x => x.Scopes, new List<string>(new[] { "test.scope.api" }));
            With(x => x.ClientSecret, new ClientSecretDto { Name = "secret-name", Vault = "secret-vault" });
        }
    }
}