using System;
using System.Collections.Generic;
using System.Text;

namespace CaptainHook.Common.Telemetry.Message
{
    public class UnroutableMessageEvent
    {
        public string EventType { get; set; }
        public string Selector { get; set; }
        public string SubscriberName { get; set; }
        public string Message { get; set; }
    }
}
