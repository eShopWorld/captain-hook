using CaptainHook.Domain.Results;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class SubscribersProcessor : ISubscribersProcessor
    {
        private readonly BuildCaptainHookProxyDelegate _captainHookBuilder;
        private readonly IConsoleSubscriberWriter _writer;


        public SubscribersProcessor(IConsoleSubscriberWriter writer, BuildCaptainHookProxyDelegate captainHookBuilder)
        {
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
            _captainHookBuilder = captainHookBuilder ?? throw new ArgumentNullException(nameof(captainHookBuilder));
        }

        public async Task<List<OperationResult<HttpOperationResponse>>> ConfigureEdaAsync(IEnumerable<PutSubscriberFile> subscriberFiles, string environment)
        {
            var api = _captainHookBuilder(environment);
            var apiResults = new List<OperationResult<HttpOperationResponse>>();

            await foreach (var apiResult in api.CallApiAsync(subscriberFiles))
            {
                var apiResultResponse = apiResult.Response;
                apiResults.Add(apiResultResponse);

                if (apiResultResponse.IsError)
                {
                    _writer.WriteError($"Error when processing '{apiResult.Filename}', Subscriber '{apiResult.Request.SubscriberName}':", apiResultResponse.Error.Message);
                }
                else
                {
                    _writer.WriteSuccess($"File '{apiResult.Filename}', Subscriber '{apiResult.Request.SubscriberName}' has been processed successfully");
                }
            }

            var totalRequestsCount = subscriberFiles.Count();
            var successfulRequestsCount = apiResults.Count(x => !x.IsError);

            _writer.WriteSuccess("box", $"Processed {totalRequestsCount} requests, {successfulRequestsCount} successfully completed, {totalRequestsCount - successfulRequestsCount} with errors.");

            return apiResults;
        }
    }
}
