namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthHandlerFactory
    {
        IAcquireTokenHandler Get(string name);
    }
}
