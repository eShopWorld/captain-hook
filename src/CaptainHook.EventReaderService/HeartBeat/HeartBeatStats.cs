using System;
using System.Fabric;

namespace CaptainHook.EventReaderService.HeartBeat
{
    public class HeartBeatStats
    {
        private readonly object _syncObject = new object();

        private int _numberOfMessagesInFlight;

        private int _numberOfAvailableHandlers;

        private DateTime? _lastTimeMessagesReadUtc;

        private int _numberOfMessagesReadLastTime;

        private int _numberOfMessagesReadSinceLastHeartBeat;

        private int _numberOfTimesNoMessagesReadSinceLastHeartBeat;

        private int _numberOfDeliveredMessagesSinceLastHeartBeat;

        private int _numberOfUndeliveredMessagesSinceLastHeartBeat;

        private readonly bool _enabled;

        public HeartBeatStats(bool enabled)
        {
            _enabled = enabled;
        }

        public void ReportMessagesRead(int numberOfMessagesRead)
        {
            if (!_enabled)
            {
                return;
            }

            lock (_syncObject)
            {
                _lastTimeMessagesReadUtc = DateTime.UtcNow;
                _numberOfMessagesReadLastTime = numberOfMessagesRead;

                _numberOfMessagesReadSinceLastHeartBeat += numberOfMessagesRead;
                if (numberOfMessagesRead == 0)
                {
                    _numberOfTimesNoMessagesReadSinceLastHeartBeat++;
                }
            }
        }


        public void ReportInFlight(int inflightMessagesCount, int handlerCount)
        {
            if (!_enabled)
            {
                return;
            }

            lock (_syncObject)
            {
                _numberOfMessagesInFlight = inflightMessagesCount;
                _numberOfAvailableHandlers = handlerCount;
            }
        }

        public ReaderHeartBeatEvent ToTelemetryEvent(StatefulServiceContext context)
        {
            if (!_enabled)
            {
                throw new NotSupportedException($"Heart Beat monitoring is disabled for service: {context.ServiceName}");
            }

            lock (_syncObject)
            {
                var telemetryEvent = new ReaderHeartBeatEvent(context)
                {
                    NumberOfMessagesInFlight = _numberOfMessagesInFlight,
                    NumberOfAvailableHandlers = _numberOfAvailableHandlers,
                    LastTimeMessagesReadUtc = _lastTimeMessagesReadUtc,
                    NumberOfMessagesReadLastTime = _numberOfMessagesReadLastTime,
                    NumberOfTimesNoMessagesReadSinceLastHeartBeat = _numberOfTimesNoMessagesReadSinceLastHeartBeat,
                    NumberOfMessagesReadSinceLastHeartBeat = _numberOfMessagesReadSinceLastHeartBeat,
                    NumberOfMessagesDeliveredSinceLastHeartBeat = _numberOfDeliveredMessagesSinceLastHeartBeat,
                    NumberOfMessagesUndeliveredSinceLastHeartBeat = _numberOfUndeliveredMessagesSinceLastHeartBeat
                };

                this.Reset();
                return telemetryEvent;
            }
        }

        public void ReportCompleteMessage(bool messageDelivered)
        {
            if (!_enabled)
            {
                return;
            }

            lock (_syncObject)
            {
                if (messageDelivered)
                {
                    _numberOfDeliveredMessagesSinceLastHeartBeat++;
                }
                else
                {
                    _numberOfUndeliveredMessagesSinceLastHeartBeat++;
                }
            }
        }

        private void Reset()
        {
            _numberOfMessagesReadSinceLastHeartBeat =
            _numberOfTimesNoMessagesReadSinceLastHeartBeat =
            _numberOfUndeliveredMessagesSinceLastHeartBeat =
            _numberOfDeliveredMessagesSinceLastHeartBeat = 0;
        }
    }
}