using System.Collections.Generic;
using System.Linq;
using CaptainHook.Domain.Results;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    public class ConfigurationLoadErrorEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string[] Failures { get; set; }

        public ConfigurationLoadErrorEvent(ErrorBase error)
        {
            Message = error?.Message;
            Failures = error?.Failures.Select(x => x.ToString()).ToArray();
        }
    }
}
