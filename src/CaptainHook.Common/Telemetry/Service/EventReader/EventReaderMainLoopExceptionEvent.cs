using Eshopworld.Core;
using System;

namespace CaptainHook.Common.Telemetry.Service.EventReader
{
    public class EventReaderMainLoopExceptionEvent : ExceptionEvent
    {
        public EventReaderMainLoopExceptionEvent(Exception exception) : base(exception)
        {
        }
    }
}
