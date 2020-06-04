﻿using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    class ServiceCreatedEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string ReaderName { get; set; }
        public string Configuration { get; set; }

        public ServiceCreatedEvent(string readerName, string configuration)
        {
            ReaderName = readerName;
            Configuration = configuration;
            Message = $"Created Service: {readerName}";
        }
    }
}