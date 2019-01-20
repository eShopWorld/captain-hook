namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IHandlerFactory
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        IHandler CreateHandler(string name);
    }
}
