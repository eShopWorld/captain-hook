using Autofac.Features.Indexed;
using CaptainHook.Common;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthHandlerFactory
    {
        IAuthHandler Get(string name);
    }

    public class AuthHandlerFactory : IAuthHandlerFactory
    {
        private readonly IIndex<string, WebhookConfig> _webHookConfigs;

        public AuthHandlerFactory(IIndex<string, WebhookConfig> webHookConfigs)
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
                        return new MmAuthenticationHandler(config.AuthenticationConfig);
                    default:
                        return new AuthenticationHandler(config.AuthenticationConfig);
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
