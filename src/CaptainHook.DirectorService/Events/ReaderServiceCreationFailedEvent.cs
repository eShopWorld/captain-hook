using System;
using Eshopworld.Core;

namespace CaptainHook.DirectorService.Events
{
    public class ReaderServiceCreationFailedEvent: ExceptionEvent
    {
        public string ReaderName { get; }

        public ReaderServiceCreationFailedEvent (string readerName, Exception e): 
            base (e)
        {
            ReaderName = readerName;
        }
    }
}