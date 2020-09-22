using System;

namespace CaptainHook.Common.Telemetry.Service
{
    [Serializable]
    public class LockTokenNotFoundException : Exception
    {
        public LockTokenNotFoundException(string message) : base(message)
        {}

        public string EventType { get; set; }

        public int HandlerId { get; set; }

        public string CorrelationId { get; set; }
    }
}
