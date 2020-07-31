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
        private TestsConfig TestsConfig { get; set; }

        private readonly TimeSpan _defaultPollTimeSpan = TimeSpan.FromMinutes(5);
        private readonly TimeSpan _defaultPollAttemptRetryTimeSpan = TimeSpan.FromMilliseconds(200);

        public E2EFlowTestsFixture()
        {
            SetupFixture();
        }

        public void SetupFixture()
        {
            TestsConfig = GetTestsConfig(); // loads from different KVs for Development and CI environment

            Bb = BigBrother.CreateDefault(TestsConfig.InstrumentationKey, TestsConfig.InstrumentationKey);
            Bb.PublishEventsToTopics(new Messenger(TestsConfig.ServiceBusConnectionString, TestsConfig.AzureSubscriptionId));
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
                        $"{TestsConfig.PeterPanBaseUrl}/api/v1/inttest/check/{payloadId}");

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

        /// <summary>
        /// run a flow and test that no actual tracked event is registered
        ///
        /// please note that due to its nature, this will use up the full timeout and there is risk of false positive - that is no tracked event registered due to CH "bug"
        /// </summary>
        /// <typeparam name="T">type of triggering event</typeparam>
        /// <param name="instance">instance of triggering event</param>
        /// <param name="waitTimespan">(optional) timeout to be used</param>
        /// <returns>async task</returns>
        public async Task ExpectNoTrackedEvent<T>(T instance, TimeSpan waitTimespan = default) where T : FlowTestEventBase
        {
            var processedEvents = await PublishAndPoll(instance, waitTimespan, false);

            processedEvents.Should().BeNullOrEmpty();
        }

        /// <summary>
        /// run a flow, expect actual events being tracked @ PeterPan and check the tracked event data
        /// </summary>
        /// <typeparam name="T">type of triggering event</typeparam>
        /// <param name="instance">instance of triggering event</param>
        /// <param name="configTestBuilder">builder for test predicate</param>
        /// <param name="waitTimespan">(optional) timeout to be used</param>
        /// <returns>async task</returns>
        public async Task ExpectTrackedEvent<T>(T instance, Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> configTestBuilder, TimeSpan waitTimespan = default) where T : FlowTestEventBase
        {
            var processedEvents = await PublishAndPoll(instance, waitTimespan);

            var predicate = new FlowTestPredicateBuilder();
            predicate = configTestBuilder.Invoke(predicate);

            processedEvents.Should().OnlyContain(m => predicate.BuildMatchesAll().Invoke(m));
        }

        /// <summary>
        /// run a flow, expect actual events being tracked @ PeterPan and check the tracked event data for webhook and callback
        /// </summary>
        /// <typeparam name="T">type of triggering event</typeparam>
        /// <param name="instance">instance of triggering event</param>
        /// <param name="configTestBuilderHook">builder for test predicate for webhook part</param>
        /// <param name="configTestBuilderCallback">builder for test predicate for callback part</param>
        /// <param name="waitTimespan">(optional) timeout to be used</param>
        /// <returns>async task</returns>
        public async Task ExpectTrackedEventWithCallback<T>(T instance, Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> configTestBuilderHook, Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> configTestBuilderCallback, TimeSpan waitTimespan = default) where T : FlowTestEventBase
        {
            var processedEvents = await PublishAndPoll(instance, waitTimespan, waitForCallback:true);

            var predicate = new FlowTestPredicateBuilder();
            predicate = configTestBuilderHook.Invoke(predicate);

            var callbackPredicate = new FlowTestPredicateBuilder();
            callbackPredicate = configTestBuilderCallback.Invoke(callbackPredicate);

            var processedEventModels = processedEvents as ProcessedEventModel[] ?? processedEvents.ToArray();

            processedEventModels.Where(m=> !m.IsCallback).Should().Contain(m => predicate.BuildMatchesAll().Invoke(m));
            processedEventModels.Where(m => m.IsCallback).Should().Contain(m => callbackPredicate.BuildMatchesAll().Invoke(m));
        }

        private async Task<IEnumerable<ProcessedEventModel>> PublishAndPoll<T>(T instance, TimeSpan waitTimespan, bool expectMessages = true, bool waitForCallback=false) where T : FlowTestEventBase
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
                ClientId = TestsConfig.StsClientId,
                ClientSecret = TestsConfig.ApiSecret,
                GrantType = "client_credentials",
                Scope = PeterPanConsts.PeterPanDeliveryScope
            });

            return response;
        }

    }
}
