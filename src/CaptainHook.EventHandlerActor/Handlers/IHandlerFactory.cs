namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHandlerFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="brandType"></param>
        /// <param name="domainType"></param>
        /// <returns></returns>
        IHandler CreateHandler(string brandType, string domainType);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="providerName">HTTP Client provider with cached auth</param>
        /// <param name="key">To get callback configuration</param>
        /// <returns></returns>
        IHandler CreateCallbackHandler(string providerName, string key);
    }
}