using System.Collections.Generic;
using System.Linq;
using Eshopworld.Core;
using Newtonsoft.Json;

namespace CaptainHook.DirectorService.Events
{
    public class ReaderServicesDeletionEvent : TimedTelemetryEvent
    {
        public string DeletedNames { get; private set; }
        public string Failed { get; private set; }

        public void SetDeletedNames (IEnumerable<string> deletedNames)
        {
            DeletedNames = JsonConvert.SerializeObject (deletedNames.ToArray ());
        }

        public void SetFailed (IEnumerable<FailedReaderServiceDeletion> failed)
        {
            Failed = JsonConvert.SerializeObject (failed.ToArray ());
        }
    }

    public readonly struct FailedReaderServiceDeletion
    {
        public readonly string Name;
        public readonly string ErrorMessage;

        public FailedReaderServiceDeletion (string name, string errorMessage)
        {
            Name = name;
            ErrorMessage = errorMessage;
        }
    }

}