using System.Collections.Generic;
using System.Linq;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    public class RefreshSubscribersEvent : TelemetryEvent
    {
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int ChangedCount { get; set; }
        public string AddedReaders { get; set; }
        public string RemovedReaders { get; set; }
        public string ChangedReaders { get; set; }

        public RefreshSubscribersEvent(IEnumerable<string> added, IEnumerable<string> removed, IEnumerable<string> changed)
        {
            AddedCount = added.Count();
            RemovedCount = removed.Count();
            ChangedCount = changed.Count();

            AddedReaders = JsonConvert.SerializeObject(added);
            RemovedReaders = JsonConvert.SerializeObject(removed);
            ChangedReaders = JsonConvert.SerializeObject(changed);
        }
    }
}