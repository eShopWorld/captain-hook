using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eshopworld.Core;
using Eshopworld.Messaging;
using Eshopworld.Telemetry;
using FluentAssertions;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Newtonsoft.Json;
using Polly;
using Xunit;

namespace CaptainHook.Tests.Web.FlowTests
{
    /// <summary>
    /// test fixture for E2E flow tests (int tests) for Captain Hook
    ///
    /// provides reusable scaffolding for specific tests
    /// </summary>
    public class E2EFlowTestsFixture
    {
        private IBigBrother _bb;
        private string peterPanUrlBase { get; set; }

        public E2EFlowTestsFixture()
        {
            SetupFixture();
        }

        private void SetupFixture()
        {
#if (!LOCAL)
            var config = new ConfigurationBuilder().AddAzureKeyVault(
                "https://esw-tooling-ci-we.vault.azure.net/",
                new KeyVaultClient(
                    new KeyVaultClient.AuthenticationCallback(new AzureServiceTokenProvider()
                        .KeyVaultTokenCallback)),
                new DefaultKeyVaultSecretManager()).Build();

            var instrKey = config["ApplicationInsights:InstrumentationKey"];
            var sbConnString = config["SB:eda:ConnectionString"];
            var subId = config["Environment:SubscriptionId"];
            
            peterPanUrlBase = config["Platform:PlatformPeterpanApi:Cluster"];
#else
            //for local testing to bypass KV load

            var instrKey = "";
            var sbConnString = "";
            var subId = "";
            peterPanUrlBase = "";
#endif

            _bb = new BigBrother(instrKey, instrKey);
            _bb.PublishEventsToTopics(new Messenger(sbConnString, subId));
        }

        public string PublishModel<T>(T raw) where T : FlowTestEventBase
        {
            var payloadId = Guid.NewGuid().ToString();
            raw.PayloadId = payloadId;

            _bb.Publish(raw);

            return payloadId;
        }

        public async Task<IEnumerable<ProcessedEventModel>> GetProcessedEvents(string payloadId)
        {
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));
            var retry = Policy
                .HandleResult<HttpResponseMessage>(msg => msg.StatusCode == HttpStatusCode.NoContent)
                .Or<Exception>()
                .WaitAndRetryForeverAsync((i, context) => TimeSpan.FromMilliseconds(100));

            var policy = timeout.WrapAsync(retry);

            var result = await policy.ExecuteAsync(() =>
            {
                var httpClient = new HttpClient();

                return httpClient.GetAsync(
                    $"{peterPanUrlBase}/api/v1/inttest/check/{payloadId}");
            });

            result.EnsureSuccessStatusCode();
            var content = await result.Content.ReadAsStringAsync();
            var items= JsonSerializer.CreateDefault()
                .Deserialize<ProcessedEventModel[]>(new JsonTextReader(new StringReader(content)));

            return items;
        }

        public async Task RunMessageFlow<T>(T instance, Func<FlowTestPredicateBuilder, FlowTestPredicateBuilder> configTestBuilder)
        {
            var payloadId = PublishModel(new HookFlowTestEvent());
            var processedEvents = await GetProcessedEvents(payloadId);

            var predicate = new FlowTestPredicateBuilder();
            predicate = configTestBuilder.Invoke(predicate);

            processedEvents.Should().OnlyContain(m => predicate.Build().Invoke(m));
        }
    }

    [CollectionDefinition(nameof(E2EFlowTestsCollection))]
    // ReSharper disable once InconsistentNaming
    public class E2EFlowTestsCollection : ICollectionFixture<E2EFlowTestsFixture>
    { }
}
