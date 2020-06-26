﻿using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    public class ServiceCreatedEvent : TelemetryEvent
    {
        public string ReaderName { get; set; }

        public ServiceCreatedEvent(string readerName)
        {
            ReaderName = readerName;
        }
    }
}