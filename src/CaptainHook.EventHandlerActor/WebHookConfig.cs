namespace CaptainHook.EventHandlerActor
{
    using Handlers.Authentication;

    public class WebHookConfig
    {
        public string Uri { get; set; }

        public bool RequiresAuth { get; set; } = true;

        public AuthConfig AuthConfig { get; set; }
    }
}