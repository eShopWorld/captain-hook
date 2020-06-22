using CaptainHook.DirectorService.Infrastructure;
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

        public RefreshSubscribersEvent(SubscriberConfigurationComparer.Result result)
        {
            AddedCount = result.Added.Count;
            RemovedCount = result.Removed.Count;
            ChangedCount = result.Changed.Count;

            AddedReaders = JsonConvert.SerializeObject(result.Added.Keys);
            RemovedReaders = JsonConvert.SerializeObject(result.Removed.Keys);
            ChangedReaders = JsonConvert.SerializeObject(result.Changed.Keys);
        }
    }
}