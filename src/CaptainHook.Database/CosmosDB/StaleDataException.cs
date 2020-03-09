using System;
using System.Runtime.Serialization;

namespace CaptainHook.Database.CosmosDB
{
    [Serializable]
    public class StaleDataException: Exception 
    {
        public StaleDataException () { }

        protected StaleDataException (SerializationInfo info, StreamingContext context)
            :base (info, context)
        {
        }
    }
}