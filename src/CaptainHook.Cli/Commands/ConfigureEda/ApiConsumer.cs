using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
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
        private static readonly HttpStatusCode[] ValidResponseCodes = { HttpStatusCode.Created, HttpStatusCode.Accepted };

        private readonly ICaptainHookClient _captainHookClient;

        private readonly AsyncRetryPolicy<HttpOperationResponse> _putRequestRetryPolicy;

        public ApiConsumer(ICaptainHookClient captainHookClient)
        {
            _captainHookClient = captainHookClient;
            _putRequestRetryPolicy = RetryUntilStatus(ValidResponseCodes);
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

                var lastResponseValid = ValidResponseCodes.Contains(response.Response.StatusCode);
                if (lastResponseValid)
                {
                    yield return new ApiOperationResult
                    {
                        File = file.File,
                        Response = response
                    };
                }

                yield return new ApiOperationResult
                {
                    File = file.File,
                    Response = await BuildExecutionError(response.Response)
                };
            }
        }

        private static async Task<CliExecutionError> BuildExecutionError(HttpResponseMessage response)
        {
            var message = new string[]
            {
                $"Status code: {response.StatusCode:D}",
                $"Reason: {response.ReasonPhrase}",
                $"Response: {await response.Content.ReadAsStringAsync()}"
            };
            return new CliExecutionError(string.Join(Environment.NewLine, message));
        }

        private static AsyncRetryPolicy<HttpOperationResponse> RetryUntilStatus(params HttpStatusCode[] acceptableHttpStatusCodes)
        {
            return Policy /* poll until desired status */
                .HandleResult<HttpOperationResponse>(msg => !acceptableHttpStatusCodes.Contains(msg.Response.StatusCode))
                .WaitAndRetryAsync(new[]
                {
                    TimeSpan.FromSeconds(3.0),
                    TimeSpan.FromSeconds(6.0)
                });
        }
    }
}