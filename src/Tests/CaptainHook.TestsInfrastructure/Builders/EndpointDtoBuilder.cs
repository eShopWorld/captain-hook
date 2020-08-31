using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class EndpointDtoBuilder : SimpleBuilder<EndpointDto>
    {
        public EndpointDtoBuilder()
        {
            With(e => e.Uri, "https://blah.blah.eshopworld.com/webhook/");
            With(e => e.HttpVerb, "POST");
            With(e => e.Authentication, new OidcAuthenticationDtoBuilder().Create());
        }
    }
}