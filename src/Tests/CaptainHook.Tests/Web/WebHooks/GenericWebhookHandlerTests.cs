using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.EventHandlerActor.Handlers;
using CaptainHook.EventHandlerActor.Handlers.Authentication;
using CaptainHook.EventHandlerActor.Handlers.Requests;
using CaptainHook.Tests.Web.Authentication;
using Eshopworld.Core;
using Eshopworld.Platform.Messages;
using Eshopworld.Platform.Messages.Enums;
using Eshopworld.Tests.Core;
using FluentAssertions;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class GenericWebhookHandlerTests
    {
        private readonly CancellationToken _cancellationToken;
        private readonly ConfigurationSettings _configurationSettings;

        public GenericWebhookHandlerTests()
        {
            _cancellationToken = new CancellationToken();
            _configurationSettings = new ConfigurationSettings();
        }

        [IsUnit]
        [Fact]
        public async Task ExecuteHappyPathRawContract()
        {
            var (messageData, metaData) = EventHandlerTestHelper.CreateMessageDataPayload();

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                HttpMethod = HttpMethod.Put,
                EventType = "Event1",
                AuthenticationConfig = new AuthenticationConfig(),
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                   new WebhookRequestRule
                   {
                       Source = new SourceParserLocation
                       {
                           Path = "OrderCode"
                       },
                       Destination = new ParserLocation
                       {
                           Location = Location.Uri
                       }
                   }
                }
            };

            var mockHttp = new MockHttpMessageHandler();
            var webhookRequest = mockHttp.When(HttpMethod.Put, $"{config.Uri}/{metaData["OrderCode"]}")
                .WithContentType("application/json", messageData.Payload)
                .Respond(HttpStatusCode.OK, "application/json", string.Empty);

            var mockBigBrother = new Mock<IBigBrother>();
            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _configurationSettings);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpClientBuilder,
                new Mock<IAuthenticationHandlerFactory>().Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            var messageDelivered = await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.Equal(1, mockHttp.GetMatchCount(webhookRequest));
            Assert.True(messageDelivered);
        }

        [IsUnit]
        [Fact]
        public async Task ExecuteHappyPathRawContract_DeliveryFailure()
        {
            var (messageData, metaData) = EventHandlerTestHelper.CreateMessageDataPayload();

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                HttpMethod = HttpMethod.Put,
                EventType = "Event1",
                AuthenticationConfig = new AuthenticationConfig(),
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                   new WebhookRequestRule
                   {
                       Source = new SourceParserLocation
                       {
                           Path = "OrderCode"
                       },
                       Destination = new ParserLocation
                       {
                           Location = Location.Uri
                       }
                   }
                }
            };

            var mockHttp = new MockHttpMessageHandler();
            var webhookRequest = mockHttp.When(HttpMethod.Put, $"{config.Uri}/{metaData["OrderCode"]}")
                .WithContentType("application/json", messageData.Payload)
                .Respond(HttpStatusCode.ServiceUnavailable, "application/json", string.Empty);

            var mockBigBrother = new Mock<IBigBrother>();
            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _configurationSettings);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpClientBuilder,
                new Mock<IAuthenticationHandlerFactory>().Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            var messageDelivered = await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.Equal(3, mockHttp.GetMatchCount(webhookRequest));
            Assert.False(messageDelivered);
        }

        [IsUnit]
        [Fact]
        public async Task ExecuteDeliveryFailurePath()
        {
            var (messageData, metaData) = EventHandlerTestHelper.CreateMessageDataPayload();

            messageData.IsDlq = true;

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                HttpMethod = HttpMethod.Put,
                EventType = "Event1",
                PayloadTransformation = PayloadContractTypeEnum.WrapperContract,
                AuthenticationConfig = new AuthenticationConfig(),
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                   new WebhookRequestRule
                   {
                       Source = new SourceParserLocation
                       {
                           Path = "OrderCode"
                       },
                       Destination = new ParserLocation
                       {
                           Location = Location.Uri
                       }
                   }
                }
            };

            var mockHttp = new MockHttpMessageHandler();

            var dlqEndpointRequest = mockHttp.Expect(HttpMethod.Put, $"{config.Uri}/{metaData["OrderCode"]}")
                .With((m) =>
                {
                    //check event type header
                    IEnumerable<string> evTypeValues = new List<string>();
                    var evType = m.Headers.TryGetValues(Constants.Headers.EventType, out evTypeValues);
                    evTypeValues.Should().Contain(typeof(NewtonsoftDeliveryStatusMessage).FullName.ToLowerInvariant());
                    //check content to match the DLQ contract
                    var str = m.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    using (var sr = new StringReader(str))
                    {
                        using (var jr = new JsonTextReader(sr))
                        {
                            var wrapperObj = JsonSerializer.CreateDefault().Deserialize<NewtonsoftDeliveryStatusMessage>(jr);
                            return JToken.DeepEquals(wrapperObj.Payload, JObject.Parse(messageData.Payload)) && wrapperObj.CallbackType == CallbackTypeEnum.DeliveryFailure;
                        }
                    }
                })
                .Respond(HttpStatusCode.OK, "application/json", string.Empty);

            var mockBigBrother = new Mock<IBigBrother>();
            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _configurationSettings);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpClientBuilder,
                new Mock<IAuthenticationHandlerFactory>().Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);
            Assert.Equal(1, mockHttp.GetMatchCount(dlqEndpointRequest));
        }

        [Fact, IsUnit]
        public async Task CallAsync_WithModelTransformWithoutRoutes_CreatesCorrectPayload()
        {
            var payloadStr = @"
        {""TenantCode"":""TARTEC"",
            ""Response"":{
                ""TenantCode"":""TARTEC"",
                ""BrandOrderReference"":""37219693"",
                ""EShopWorldOrderNumber"":""5001014519956"",
                ""DeliveryCountryIso"":""AU"",
                ""TransactionReference"":""00000000 -0000-0000-0000-000000000000"",
                ""Code"":0,
                ""Message"":null
            },
            ""CallerMemberName"":null,
            ""CallerFilePath"":null,
            ""CallerLineNumber"":0
        }";
            var jPayload = JsonConvert.DeserializeObject(payloadStr) as JObject;
            var payload = JsonConvert.SerializeObject(jPayload, Formatting.None);

            var messageData = new MessageData(payload, "test-type", "invoice", "replyTo")
            {
                CorrelationId = Guid.NewGuid().ToString(),
                ServiceBusMessageId = Guid.NewGuid().ToString()
            };

            var config = new WebhookConfig
            {
                Uri = "http://localhost/webhook",
                HttpMethod = HttpMethod.Post,
                EventType = "Event1",
                AuthenticationConfig = new AuthenticationConfig(),
                WebhookRequestRules = new List<WebhookRequestRule>
                {
                   new WebhookRequestRule
                   {
                       Source = new SourceParserLocation
                       {
                           Path = "$.Response",
                           Location = Location.Body,
                           RuleAction = RuleAction.Add,
                           Type = DataType.Model
                       },
                       Destination = new ParserLocation
                       {
                           Path = null,
                           Location = Location.Body,
                           RuleAction = RuleAction.Add,
                           Type = DataType.Model
                       }
                   }
                }
            };

            var expectedPayload = jPayload["Response"].ToString(Formatting.None);
            var mockHttp = new MockHttpMessageHandler();
            var webhookRequest = mockHttp.When(HttpMethod.Post, $"{config.Uri}")
                .WithContentType("application/json", expectedPayload)
                .Respond(HttpStatusCode.OK, "application/json", string.Empty);

            var mockBigBrother = new Mock<IBigBrother>();
            var httpClients = new Dictionary<string, HttpClient> { { new Uri(config.Uri).Host, mockHttp.ToHttpClient() } };

            var httpClientBuilder = new HttpClientFactory(httpClients);
            var requestBuilder = new DefaultRequestBuilder(Mock.Of<IBigBrother>());
            var requestLogger = new RequestLogger(mockBigBrother.Object, _configurationSettings);

            var genericWebhookHandler = new GenericWebhookHandler(
                httpClientBuilder,
                new Mock<IAuthenticationHandlerFactory>().Object,
                requestBuilder,
                requestLogger,
                mockBigBrother.Object,
                config);

            var messageDelivered = await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

            Assert.Equal(1, mockHttp.GetMatchCount(webhookRequest));
            Assert.True(messageDelivered);
        }


    }
}
