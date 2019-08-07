using System;

namespace CaptainHook.Common
{
    public class MessageData
    {
        public Guid Handle { get; set; }

        public string Payload { get; set; }

        public string Type { get; set; }

        /// <summary>
        /// Temp means to wire flows together until end to end actor telemetry tracking is complete
        /// </summary>
        public string CorrelationId { get; set; }
    }
}
