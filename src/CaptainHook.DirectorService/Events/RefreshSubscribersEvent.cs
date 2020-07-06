using System.Collections.Generic;
using System.Linq;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    public class RefreshSubscribersEvent : TelemetryEvent
    {
        public bool InRelease { get; set; }
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int ChangedCount { get; set; }
        public int ToCreateCount { get; set; }
        public int ToDeleteCount { get; set; }
        public string AddedReaders { get; set; }
        public string RemovedReaders { get; set; }
        public string ChangedReaders { get; set; }
        public string ReadersToCreate { get; set; }
        public string ReadersToDelete { get; set; }

        public RefreshSubscribersEvent(IEnumerable<string> added, IEnumerable<string> removed, IEnumerable<string> changed,
            IEnumerable<string> readersToCreate, IEnumerable<string> readersToDelete, bool inRelease)
        {
            AddedCount = added.Count();
            RemovedCount = removed.Count();
            ChangedCount = changed.Count();
            ToCreateCount = readersToCreate.Count();
            ToDeleteCount = readersToDelete.Count();

            AddedReaders = JsonConvert.SerializeObject(added);
            RemovedReaders = JsonConvert.SerializeObject(removed);
            ChangedReaders = JsonConvert.SerializeObject(changed);
            ReadersToCreate = JsonConvert.SerializeObject(readersToCreate);
            ReadersToDelete = JsonConvert.SerializeObject(readersToDelete);

            InRelease = inRelease;
        }
    }
}