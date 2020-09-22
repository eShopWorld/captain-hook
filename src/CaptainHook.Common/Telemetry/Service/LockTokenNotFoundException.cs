using System;
using System.Runtime.Serialization;

namespace CaptainHook.Common.Telemetry.Service
{
    [Serializable]
    public class LockTokenNotFoundException : Exception, ISerializable
    {
        public LockTokenNotFoundException(string message) : base(message)
        {}

        private LockTokenNotFoundException(SerializationInfo info, StreamingContext context)
        {
        }

        public string EventType { get; set; }

        public int HandlerId { get; set; }

        public string CorrelationId { get; set; }
    }
}
