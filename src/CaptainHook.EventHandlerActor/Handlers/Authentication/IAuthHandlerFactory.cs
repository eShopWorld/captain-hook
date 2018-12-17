namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    using Autofac.Features.Indexed;
    using Common;

    public interface IAuthHandlerFactory
    {
        IAuthHandler Get(string name);
    }

    public class AuthHandlerFactory : IAuthHandlerFactory
    {
        private readonly IIndex<string, WebHookConfig> _webHookConfigs;

        public AuthHandlerFactory(IIndex<string, WebHookConfig> webHookConfigs)
        {
            _webHookConfigs = webHookConfigs;
        }

        public IAuthHandler Get(string name)
        {
            if(_webHookConfigs.TryGetValue(name.ToLower(), out var config))
            {
                switch (name.ToLower())
                {
                    case "max":
                    case "dif":
                        return new MmAuthHandler(config.Auth);
                    default:
                        return new AuthHandler(config.Auth);
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