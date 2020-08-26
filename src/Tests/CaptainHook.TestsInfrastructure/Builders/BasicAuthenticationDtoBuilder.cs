using CaptainHook.Contract;

namespace CaptainHook.TestsInfrastructure.Builders
{
    public class BasicAuthenticationDtoBuilder : SimpleBuilder<BasicAuthenticationDto>
    {
        public BasicAuthenticationDtoBuilder()
        {
            With(x => x.AuthenticationType, "Basic");
            With(x => x.Username, "Batman");
            With(x => x.Password, "Batcave");
        }
    }
}
