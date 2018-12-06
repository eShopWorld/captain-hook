namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHandlerFactory
    {
        IHandler CreateHandler(string brandType, string domainType);
    }
}