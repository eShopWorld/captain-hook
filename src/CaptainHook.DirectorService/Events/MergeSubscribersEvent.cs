using System.Collections.Generic;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    public class MergeSubscribersEvent : TelemetryEvent
    {
        public string OnlyInKeyVault { get; set; }

        public MergeSubscribersEvent(IEnumerable<string> onlyInKeyVault)
        {
            OnlyInKeyVault = JsonConvert.SerializeObject(onlyInKeyVault);
        }
    }
}