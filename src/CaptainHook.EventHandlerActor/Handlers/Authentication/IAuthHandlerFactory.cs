namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using Autofac.Features.Indexed;

    public interface IAuthHandlerFactory
    {
        IAuthHandler Get(string name);
    }

    public class AuthHandlerFactory : IAuthHandlerFactory
    {
        private readonly IIndex<string, AuthConfig> _authConfigs;

        public AuthHandlerFactory(IIndex<string, AuthConfig> authConfigs)
        {
            _authConfigs = authConfigs;
        }

        public IAuthHandler Get(string name)
        {
            if(_authConfigs.TryGetValue(name, out var config))
            {
                switch (name)
                {
                    case "MAX":
                    case "DIF":
                        return new MmAuthHandler(config);
                    default:
                        return new AuthHandler(config);
                }
            }
            else
            {
                //todo handle unknown auth config
            }

            return null;
        }
    }
}