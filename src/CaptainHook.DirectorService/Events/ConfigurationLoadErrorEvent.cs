using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    [ExcludeFromCodeCoverage]
    public class ConfigurationLoadErrorEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string Failures { get; set; }

        public ConfigurationLoadErrorEvent(IEnumerable<ErrorBase> errors)
        {
            Failures = JsonConvert.SerializeObject(errors);
        }
    }
}
