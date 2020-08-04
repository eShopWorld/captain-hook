using CaptainHook.Tests.Configuration;
using Eshopworld.Core;
using Eshopworld.DevOps;
using Eshopworld.Messaging;
using Eshopworld.Telemetry;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Polly;
using Polly.Timeout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace CaptainHook.Tests.Web.FlowTests
{
    /// <summary>
    /// test fixture for E2E flow tests (int tests) for Captain Hook
    ///
    /// provides reusable scaffolding for specific tests
    /// </summary>
    public class E2EFlowTestsFixture
    {
        public IBigBrother Bb;
        private readonly TestsConfig _testsConfig;

        private readonly TimeSpan _defaultPollTimeSpan = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _defaultPollAttemptRetryTimeSpan = TimeSpan.FromMilliseconds(200);

        public E2EFlowTestsFixture()
        {
            _testsConfig = GetTestsConfig(); // loads from different KVs for Development and CI environment

            SetupFixture();
        }

        public void SetupFixture()
        {
            Bb = BigBrother.CreateDefault(_testsConfig.InstrumentationKey, _testsConfig.InstrumentationKey);
            Bb.PublishEventsToTopics(new Messenger(_testsConfig.ServiceBusConnectionString, _testsConfig.AzureSubscriptionId));
        }

        /// <summary>
        /// Loads configuration parameters for Integrations tests from AppSettings.json files 
        /// and secrets from the KeyvaultUrl configured in appsettings
        /// </summary>
        /// <returns><see cref="Configuration.TestsConfig"/> object</returns>
        private TestsConfig GetTestsConfig()
        {
            var config = EswDevOpsSdk.BuildConfiguration(); 
            var testsConfig = new TestsConfig();

            // Load: InstrumentationKey and AzureSubscriptionId from KV; 
            // PeterPanBaseUrl and StsClientId from appsettings
            config.Bind(testsConfig);

            // Binds CaptainHook:ServiceBusConnectionString, CaptainHook:ApiSecret from KV
            config.Bind("CaptainHook", testsConfig);

            return testsConfig;
        }

        private string PublishModel<T>(T raw) where T : FlowTestEventBase
        {
            var payloadId = Guid.NewGuid().ToString();
            raw.PayloadId = payloadId;

            Bb.Publish(raw);

            return payloadId;
        }

        private async Task<IEnumerable<ProcessedEventModel>> GetProcessedEvents(string payloadId, TimeSpan timeoutTimeSpan = default, bool expectMessages = true, bool expectCallback = false)
        {
            try
            {
                ProcessedEventModel[] modelReceived = null;
                var jsonSerializer = JsonSerializer.CreateDefault();
                var timeout = Policy.TimeoutAsync(timeoutTimeSpan == default ? _defaultPollTimeSpan : timeoutTimeSpan);

                var retry = Policy
                .HandleResult<HttpResponseMessage>(msg =>
                    !expectMessages || msg.StatusCode == HttpStatusCode.NoContent || (expectCallback && (modelReceived == null || !modelReceived.Any(m => m.IsCallback))) /* keep polling */ )
                .Or<Exception>()
                .WaitAndRetryForeverAsync((i, context) => _defaultPollAttemptRetryTimeSpan);

                var policy = timeout.WrapAsync(retry);

                var token = await ObtainStsToken();
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var result = await policy.ExecuteAsync(async () =>
                {
                    var response = await httpClient.GetAsync(
                        $"{_testsConfig.PeterPanBaseUrl}/api/v1/inttest/check/{payloadId}");

                    if (response.StatusCode != HttpStatusCode.OK) return response;

                    var content = await response.Content.ReadAsStringAsync();
                    modelReceived = jsonSerializer.Deserialize<ProcessedEventModel[]>(new JsonTextReader(new StringReader(content)));

                    return response;
                });

                result.EnsureSuccessStatusCode();

                return modelReceived;
            }
            catch (TimeoutRejectedException)
            {
                if (!expectMessages) return null;

                throw;
            }
        }

        public async Task<IEnumerable<ProcessedEventModel>> PublishAndPoll<T>(T instance, TimeSpan waitTimespan = default, bool expectMessages = true, bool waitForCallback=false) where T : FlowTestEventBase
        {
            var payloadId = PublishModel(instance);
            var processedEvents = await GetProcessedEvents(payloadId, waitTimespan, expectMessages, waitForCallback);
            return processedEvents;
        }

        private async Task<string> ObtainStsToken()
        {
            using var client = new HttpClient();

            var response = await GetTokenResponseAsync(client);

            return response.AccessToken;
        }

        private async Task<TokenResponse> GetTokenResponseAsync(HttpMessageInvoker client)
        {
            var response = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = "https://security-sts.ci.eshopworld.net/connect/token",
                ClientId = _testsConfig.StsClientId,
                ClientSecret = _testsConfig.ApiSecret,
                GrantType = "client_credentials",
                Scope = PeterPanConsts.PeterPanDeliveryScope
            });

            return response;
        }

    }
}
