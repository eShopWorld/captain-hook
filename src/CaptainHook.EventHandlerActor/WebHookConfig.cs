namespace CaptainHook.EventHandlerActor
{
    using Common.Authentication;

    public class WebHookConfig
    {
        public string Uri { get; set; }

        public bool RequiresAuth { get; set; }

        public AuthConfig AuthConfig { get; set; }
    }
}