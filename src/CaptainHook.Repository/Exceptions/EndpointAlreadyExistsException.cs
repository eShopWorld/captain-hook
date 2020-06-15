using System;
using System.Runtime.Serialization;

namespace CaptainHook.Repository.Exceptions
{
    [Serializable]
    public class EndpointAlreadyExistsException : Exception
    {
        public EndpointAlreadyExistsException()
        {
        }

        public EndpointAlreadyExistsException(string message) : base(message)
        {
        }

        public EndpointAlreadyExistsException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EndpointAlreadyExistsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

    }
}
