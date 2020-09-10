using System.Linq;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    public class ConfigurationLoadErrorEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string Failures { get; set; }

        public ConfigurationLoadErrorEvent(ErrorBase error)
        {
            Message = error?.Message;
            Failures = JsonConvert.SerializeObject(error?.Failures.Select(x => x.ToString()).ToArray());
        }
    }
}
