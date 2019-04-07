using System;

namespace CaptainHook.Common
{
    public class MessageData
    {
        public Guid Handle { get; set; }

        public string Payload { get; set; }

        public string Type { get; set; }

        public string HandleAsString => Handle.ToString();
    }
}
