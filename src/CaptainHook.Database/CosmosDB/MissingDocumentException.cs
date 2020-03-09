using System;
using System.Runtime.Serialization;

namespace CaptainHook.Database.CosmosDB
{
    [Serializable]
    public class MissingDocumentException : Exception
    {
        public MissingDocumentException()
        {
        }

        public MissingDocumentException(string message) : base(message)
        {
        }

        public MissingDocumentException(string message, Exception inner) : base(message, inner)
        {
        }

        protected MissingDocumentException (SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }

    }
}
