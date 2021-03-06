﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using CaptainHook.Api.Client;
using EShopworld.Security.Services.Rest;
using Eshopworld.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using Platform.Eda.Cli.Commands.ConfigureEda.Models;
using Platform.Eda.Cli.Common;
using Polly;
using Polly.Retry;

namespace Platform.Eda.Cli.Commands.ConfigureEda
{
    public class ApiConsumer : IApiConsumer
    {
        private static readonly HttpStatusCode[] ValidResponseCodes = { HttpStatusCode.Created, HttpStatusCode.Accepted };

        private readonly ICaptainHookClient _captainHookClient;

        private readonly TimeSpan[] _sleepDurations;

        private readonly AsyncRetryPolicy<HttpOperationResponse> _putRequestRetryPolicy;

        private static readonly TimeSpan[] DefaultSleepDurations = { TimeSpan.FromSeconds(3.0), TimeSpan.FromSeconds(6.0) };

        [ExcludeFromCodeCoverage]
        public static ApiConsumer BuildApiConsumer(IHttpClientFactory clientFactory, string environment)
        {
            var configuration = new ConfigurationBuilder()
                .UseDefaultConfigs(environment: environment)
                .AddKeyVaultSecrets(new Dictionary<string, string> {
                    { "sts--sts-secret--platform-captainhook-api-client", "ConfigureEdaCommandConfig:RefreshingTokenProviderOptions:ClientSecret" }
                })
                .Build();

            var configureEdaCommandConfig = configuration
                .GetSection(nameof(ConfigureEdaCommandConfig))
                .Get<ConfigureEdaCommandConfig>();

            var tokenProvider = new RefreshingTokenProvider(
                clientFactory,
                BigBrother.CreateDefault("", ""),
                configureEdaCommandConfig.RefreshingTokenProviderOptions);

            var client = new CaptainHookClient(configureEdaCommandConfig.CaptainHookUrl, new TokenCredentials(tokenProvider));
            return new ApiConsumer(client, null);
        }

        public ApiConsumer(ICaptainHookClient captainHookClient, TimeSpan[] sleepDurations)
        {
            _captainHookClient = captainHookClient;
            _sleepDurations = sleepDurations?.Any() == true ? sleepDurations : DefaultSleepDurations;
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
                        Request = file.Request,
                        File = file.File,
                        Response = response
                    };
                }
                else
                {
                    yield return new ApiOperationResult
                    {
                        Request = file.Request,
                        File = file.File,
                        Response = await BuildExecutionErrorAsync(response.Response)
                    };
                }
            }
        }

        private static async Task<CliExecutionError> BuildExecutionErrorAsync(HttpResponseMessage response)
        {
            var responseString = "(none)";
            if (response.Content != null)
            {
                responseString = await response.Content.ReadAsStringAsync();
            }

            var message = new[]
            {
                $"Status code: {response.StatusCode:D}",
                $"Reason: {response.ReasonPhrase}",
                $"Response: {responseString}"
            };
            return new CliExecutionError(string.Join(Environment.NewLine, message));
        }

        private AsyncRetryPolicy<HttpOperationResponse> RetryUntilStatus(params HttpStatusCode[] acceptableHttpStatusCodes)
        {
            return Policy /* poll until desired status */
                .HandleResult<HttpOperationResponse>(msg => !acceptableHttpStatusCodes.Contains(msg.Response.StatusCode))
                .WaitAndRetryAsync(_sleepDurations);
        }
    }
}