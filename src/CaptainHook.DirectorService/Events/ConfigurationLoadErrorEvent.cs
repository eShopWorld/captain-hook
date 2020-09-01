using CaptainHook.Domain.Results;
using Eshopworld.Core;
using Kusto.Cloud.Platform.Utils;
using System.Collections.Generic;
using System.Linq;

namespace CaptainHook.DirectorService.Events
{
    public class ConfigurationLoadErrorEvent: TelemetryEvent
    {
        public Dictionary<string, string> Failures { get; set; }

        public ConfigurationLoadErrorEvent(ErrorBase error)
        {
            Failures = error?.Failures
                .Select((input, index) => new { index, input })
                .ToDictionary(x => $"{x.input.Code}_{x.index}", x => x.input.Message);
        }
    }
}
