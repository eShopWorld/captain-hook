using CaptainHook.Domain.Results;
using Microsoft.Rest;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class ApiOperationResult
    {
        public string Filename { get; }

        public PutSubscriberRequest Request { get; }

        public OperationResult<HttpOperationResponse> Response { get; }

        public ApiOperationResult(string filename, PutSubscriberRequest request, OperationResult<HttpOperationResponse> response)
        {
            Filename = filename;
            Request = request;
            Response = response;
        }
    }
}