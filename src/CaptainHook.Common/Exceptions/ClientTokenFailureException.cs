using System;
using System.Net;
using System.Runtime.Serialization;

namespace CaptainHook.Common.Exceptions
{
    [Serializable]
    public class ClientTokenFailureException : Exception
    {
        public ClientTokenFailureException(System.Exception e) : base("Could not get a token", e)
        { }

        protected ClientTokenFailureException(SerializationInfo info, StreamingContext context)
        {
        }

        public string ClientId { get; set; }

        public string Uri { get; set; }

        public string ErrorDescription { get; set; }

        public HttpStatusCode ErrorCode { get; set; }

        public string Error { get; set; }

        public string HttpErrorReason { get; set; }

        public string Scopes { get; set; }

        public string TokenType { get; set; }

        public string ResponsePayload { get; set; }
    }
}
