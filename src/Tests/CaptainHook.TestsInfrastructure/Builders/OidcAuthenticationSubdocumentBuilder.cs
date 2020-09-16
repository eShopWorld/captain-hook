using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class OidcAuthenticationSubdocumentBuilder : SimpleBuilder<OidcAuthenticationSubdocument>
    {
        public OidcAuthenticationSubdocumentBuilder()
        {
            With(x => x.ClientId, "clientId");
            With(x => x.Uri, "https://security-api.com/token");
            With(x => x.Scopes, new string[] { "test.scope.api" });
            With(x => x.SecretName, "secret-name");
        }
    }
}
