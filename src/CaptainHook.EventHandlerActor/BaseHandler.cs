namespace CaptainHook.EventHandlerActor
{
    using System.Threading.Tasks;

    public abstract class BaseHandler : IHandler
    {
        public abstract Task MakeCall(MessageData data);
    }
}