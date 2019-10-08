using System.Collections.Generic;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public class WebHookHeaders
    {
        public WebHookHeaders()
        {
            ContentHeaders = new Dictionary<string, string>();
            RequestHeaders = new Dictionary<string, string>();
        }
        public Dictionary<string, string> ContentHeaders { get; }

        public Dictionary<string, string> RequestHeaders { get; }


        public void AddContentHeader(string name, string value)
        {
            ContentHeaders.Add(name, value);
        }

        public void AddRequestHeader(string name, string value)
        {
            RequestHeaders.Add(name, value);
        }

        public void ClearRequestHeaders()
        {
            RequestHeaders.Clear();
        }

        public void ClearContentHeaders()
        {
            ContentHeaders.Clear();
        }
    }
}