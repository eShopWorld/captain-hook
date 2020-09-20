using System.IO;
using CaptainHook.Domain.Results;
using Microsoft.Rest;

namespace CaptainHook.Cli.Commands.ConfigureEda.Models
{
    public class ApiOperationResult
    {
        public FileInfo File { get; set; }

        public OperationResult<HttpOperationResponse> Response { get; set; }
    }
}