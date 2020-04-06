using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Eshopworld.Core;
using Eshopworld.Messaging;
using Eshopworld.Telemetry;
using FluentAssertions;
using IdentityModel.Client;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Newtonsoft.Json;
using Polly;

namespace CaptainHook.Tests.Web.FlowTests
{
    /// <summary>
    /// test fixture for E2E flow tests (int tests) for Captain Hook
    ///
    /// provides reusable scaffolding for specific tests
    /// </summary>
    public class E2EFlowTestsFixture
    {
        public static IBigBrother _bb;
        public static string PeterPanUrlBase { get; set; }
        public static string StsClientId { get; set; }
        public static string StsClientSecret { get; set; }

        static E2EFlowTestsFixture()
        {
            SetupFixture();
        }

        private static void SetupFixture()
        {
#if (!LOCAL)
            var config = new ConfigurationBuilder().AddAzureKeyVault(
                "https://esw-tooling-testing-ci.vault.azure.net/",
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            var instrKey = config["ApplicationInsights:InstrumentationKey"];
            var sbConnString = config["SB:eda:ConnectionString"];
            var subId = config["Environment:SubscriptionId"];
            
            PeterPanUrlBase = config["Platform:PlatformPeterpanApi:Cluster"];
            StsClientSecret = config["STS:EDA:ClientSecret"];
            StsClientId = config["STS:EDA:ClientId"];
#else
            //for local testing to bypass KV load

            var instrKey = "";
            var sbConnString = "";
            var subId = "";
            PeterPanUrlBase = "";
            StsClientId = "";
            StsClientSecret = "";
#endif

            _bb = new BigBrother(instrKey, instrKey);
            _bb.PublishEventsToTopics(new Messenger(sbConnString, subId));
        }

        private string PublishModel<T>(T raw) where T : FlowTestEventBase
        {
            var payloadId = Guid.NewGuid().ToString();
            raw.PayloadId = payloadId;

            _bb.Publish(raw);

            return payloadId;
        }

        private async Task<IEnumerable<ProcessedEventModel>> GetProcessedEvents(string payloadId, TimeSpan timeoutTimeSpan=default)
        {
            var timeout = Policy.TimeoutAsync(timeoutTimeSpan==default ? TimeSpan.FromMinutes(5): timeoutTimeSpan);
            var retry = Policy
                .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == HttpStatusCode.NoContent)
                .Or<Exception>()
                .WaitAndRetryForeverAsync((i, context) => TimeSpan.FromMilliseconds(100));

            var policy = timeout.WrapAsync(retry);

            var token = await ObtainStsToken();
            var result = await policy.ExecuteAsync(async () =>
            {
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                return await httpClient.GetAsync(
                    $"{PeterPanUrlBase}/api/v1/inttest/check/{payloadId}");
            });

            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var items= JsonSerializer.CreateDefault()
                .Deserialize<ProcessedEventModel[]>(new JsonTextReader(new StringReader(content)));

            return items;
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
            var processedEvents = await PublishAndPoll<T>(instance, waitTimespan);

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
        public async Task ExpectTrackedEvent<T>(T instance, Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> configTestBuilder, TimeSpan waitTimespan=default) where T: FlowTestEventBase
        {
            var processedEvents = await PublishAndPoll<T>(instance, waitTimespan);

            var predicate = new FlowTestPredicateBuilder();
            predicate = configTestBuilder.Invoke(predicate);

            processedEvents.Should().OnlyContain(m => predicate.Build().Invoke(m));
        }

        private async Task<IEnumerable<ProcessedEventModel>> PublishAndPoll<T>(T instance, TimeSpan waitTimespan) where T : FlowTestEventBase
        {
            var payloadId = PublishModel(instance);
            var processedEvents = await GetProcessedEvents(payloadId, waitTimespan);
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
                ClientId = StsClientId,
                ClientSecret = StsClientSecret,
                GrantType = "client_credentials",
                Scope = "eda.peterpan.delivery.api.all"
            });

            return response;
        }

    }
}
