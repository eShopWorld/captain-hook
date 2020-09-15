using CaptainHook.Storage.Cosmos.Models;

namespace CaptainHook.TestsInfrastructure.Builders
{
    internal class EndpointSubdocumentBuilder : SimpleBuilder<EndpointSubdocument>
    {
        public EndpointSubdocumentBuilder()
        {
            With(e => e.Uri, "https://blah.blah.eshopworld.com/webhook/");
            With(e => e.HttpVerb, "POST");
            With(e => e.Authentication, new OidcAuthenticationSubdocumentBuilder().Create());
        }
    }
}
