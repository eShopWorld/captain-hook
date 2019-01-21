namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public interface IAuthHandlerFactory
    {
        IAuthHandler Get(string name);
    }
}
