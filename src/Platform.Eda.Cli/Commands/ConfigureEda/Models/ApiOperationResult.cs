using System.IO.Abstractions;
using CaptainHook.Domain.Results;
using Microsoft.Rest;

namespace Platform.Eda.Cli.Commands.ConfigureEda.Models
{
    public class ApiOperationResult
    {
        public FileInfoBase File { get; set; }

        public OperationResult<HttpOperationResponse> Response { get; set; }

        public PutSubscriberRequest Request { get; set; }
    }
}