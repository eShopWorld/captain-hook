using System.Net;

namespace CaptainHook.Common.Exceptions
{
    public class ClientTokenFailureException : System.Exception
    {
        public ClientTokenFailureException(System.Exception e) : base("Could not get a token", e)
        { }

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
