namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="fullEventName"></param>
        /// <param name="eventType"></param>
        /// <returns></returns>
        IHandler CreateHandler(string fullEventName, string eventType);

        /// <summary>
        /// Creates a single fire and forget webhook handler
        /// </summary>
        /// <param name="webHookName"></param>
        /// <returns></returns>
        IHandler CreateHandler(string webHookName);
    }
}
