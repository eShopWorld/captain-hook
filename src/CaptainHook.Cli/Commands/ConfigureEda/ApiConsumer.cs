using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using CaptainHook.Cli.Commands.ConfigureEda.Models;
using CaptainHook.Cli.Common;
using CaptainHook.Domain.Results;
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
        public async Task<OperationResult<IEnumerable<HttpOperationResponse>>> CallApiAsync(IEnumerable<PutSubscriberRequest> requests)
        {
            var responses = new List<HttpOperationResponse>();
            foreach (var putSubscriberRequest in requests)
            {
                var response = await _putRequestRetryPolicy.ExecuteAsync(async () =>
                    await _captainHookClient.PutSuscriberWithHttpMessagesAsync(
                    putSubscriberRequest.EventName,
                    putSubscriberRequest.SubscriberName,
                    putSubscriberRequest.Subscriber));

                if (response.Response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return new OperationResult<IEnumerable<HttpOperationResponse>>(new CliExecutionError(response.Response.ToString()));
                }

                responses.Add(response);
            }

            return responses;
        }

        private static AsyncRetryPolicy<HttpOperationResponse> RetryUntilStatus(params HttpStatusCode[] acceptableHttpStatusCodes)
        {
            return Policy /* poll until desired status */
                .HandleResult<HttpOperationResponse>(msg => !acceptableHttpStatusCodes.Contains(msg.Response.StatusCode))
                .WaitAndRetryAsync(10, i => TimeSpan.FromSeconds(5));
        }
    }
}