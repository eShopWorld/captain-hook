using CaptainHook.DirectorService.Utils;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    class RefreshSubscribersEvent : TelemetryEvent
    {
        public string Message { get; set; }
        public string ReadersToAdd { get; set; }
        public string ReadersToDelete { get; set; }
        public string ReadersToRefresh { get; set; }

        public RefreshSubscribersEvent(SubscriberConfigurationComparer.Result result)
        {
            Message = $"Number of Readers to add: {result.Added.Count} to delete: {result.Removed.Count} and to refresh: {result.Changed.Count}";
            ReadersToAdd = string.Join(',', result.Added.Keys);
            ReadersToDelete = string.Join(',', result.Removed.Keys);
            ReadersToRefresh = string.Join(',', result.Changed.Keys);
        }
    }
}