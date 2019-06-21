namespace CaptainHook.EventHandlerActor.Handlers
{
    public interface IEventHandlerFactory
    {
        /// <summary>
        /// Create the custom handler such that we get a mapping from the webhook to the handler selected
        /// </summary>
        /// <param name="fullEventName"></param>
        /// <returns></returns>
        IHandler CreateEventHandler(string fullEventName);

        /// <summary>
        /// Used only for getting the callback handler
        /// </summary>
        /// <param name="webHookName"></param>
        /// <returns></returns>
        IHandler CreateWebhookHandler(string webHookName);
    }
}
