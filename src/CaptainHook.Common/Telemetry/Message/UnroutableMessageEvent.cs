using Eshopworld.Core;

namespace CaptainHook.Common.Telemetry.Message
{
    /// <summary>
    /// Custom event emitted when message seen that cannot be routed
    /// </summary>
    public class UnroutableMessageEvent : TelemetryEvent
    {
        /// <summary>
        /// type of the event
        /// </summary>
        public string EventType { get; set; }
        /// <summary>
        /// value of the selector
        /// </summary>
        public string Selector { get; set; }
        /// <summary>
        /// Name of the subscriber
        /// </summary>
        public string SubscriberName { get; set; }
        /// <summary>
        /// Reason of failure in human readable form
        /// </summary>
        public string Message { get; set; }
    }
}
