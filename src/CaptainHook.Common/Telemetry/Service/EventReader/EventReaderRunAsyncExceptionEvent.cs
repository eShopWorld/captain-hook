using Eshopworld.Core;
using System;

namespace CaptainHook.Common.Telemetry.Service.EventReader
{
    public class EventReaderRunAsyncExceptionEvent : ExceptionEvent
    {
        public EventReaderRunAsyncExceptionEvent(Exception exception) : base(exception)
        {
        }
    }
}
