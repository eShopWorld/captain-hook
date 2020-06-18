using System;
using System.Fabric;
using CaptainHook.Common.Telemetry.Service;

namespace CaptainHook.EventReaderService.HeartBeat
{
    public class ReaderHeartBeatEvent: ServiceTelemetryEvent
    {
        public ReaderHeartBeatEvent(StatefulServiceContext context): base(context)
        {
        }

        public int NumberOfMessagesInFlight { get; set; }

        public int NumberOfAvailableHandlers { get; set; }

        public DateTime? LastTimeMessagesReadUtc { get; set; }

        public int NumberOfMessagesReadLastTime { get; set; }

        public int NumberOfTimesNoMessagesReadSinceLastHeartBeat { get; set; }

        public int NumberOfMessagesReadSinceLastHeartBeat { get; set; }
    }
}