using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class BasicAuthenticationSubdocumentBuilder : SimpleBuilder<BasicAuthenticationSubdocument>
    {
        public BasicAuthenticationSubdocumentBuilder()
        {
            With(x => x.Username, "user-name");
            With(x => x.PasswordKeyName, "secret-name");
        }
    }
}
