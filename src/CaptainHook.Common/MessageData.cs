namespace CaptainHook.Common
{
    using System;

    public class MessageData
    {
        public Guid Handle { get; set; }

        public string Payload { get; set; }

        public string Type { get; set; }
    }
}
