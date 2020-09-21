using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using CaptainHook.Api.Client;
using CaptainHook.Cli.Commands.ConfigureEda.Models;
using CaptainHook.Cli.Common;
using Microsoft.Rest;
using Polly;
using Polly.Retry;

namespace CaptainHook.Cli.Commands.ConfigureEda
{
    public class ApiConsumer
    {
        private readonly ICaptainHookClient _captainHookClient;
        private readonly AsyncRetryPolicy<HttpOperationResponse> _putRequestRetryPolicy;

        public ApiConsumer(ICaptainHookClient captainHookClient)
        {
            _captainHookClient = captainHookClient;
            _putRequestRetryPolicy = RetryUntilStatus(HttpStatusCode.Created, HttpStatusCode.Accepted, HttpStatusCode.Unauthorized);
        }

        public async IAsyncEnumerable<ApiOperationResult> CallApiAsync(IEnumerable<PutSubscriberFile> files)
        {
            foreach (var file in files)
            {
                var request = file.Request;
                var response = await _putRequestRetryPolicy.ExecuteAsync(async () =>
                    await _captainHookClient.PutSuscriberWithHttpMessagesAsync(
                    request.EventName,
                    request.SubscriberName,
                    request.Subscriber));

                if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    yield return new ApiOperationResult
                    {
                        File = file.File,
                        Response = new CliExecutionError(response.Response.ToString())
                    };
                }

                yield return new ApiOperationResult
                {
                    File = file.File,
                    Response = response
                };
            }
        }

        private static AsyncRetryPolicy<HttpOperationResponse> RetryUntilStatus(params HttpStatusCode[] acceptableHttpStatusCodes)
        {
            return Policy /* poll until desired status */
                .HandleResult<HttpOperationResponse>(msg => !acceptableHttpStatusCodes.Contains(msg.Response.StatusCode))
                .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(5));
        }
    }
}