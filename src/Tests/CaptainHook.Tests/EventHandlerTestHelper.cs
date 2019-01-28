using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;

namespace CaptainHook.Tests
{
    /// <summary>
    /// Helper to structure some of the requests and responses expected
    /// </summary>
    public class EventHandlerTestHelper
    {
        //todo this should be jpath stuff
        public class Payload
        {
            public string BrandType { get; set; }

            public Guid OrderCode { get; set; }
        }

        /// <summary>
        /// Sample transport model to test data coming back from the webhook
        /// </summary>
        public class TransportModel
        {
            public string Name { get; set; }
        }

        /// <summary>
        /// Extended payload model to include a return payload from the webhook
        /// </summary>
        public class PayloadWithModel : Payload
        {
            public TransportModel TransportModel { get; set; }
        }


        public static string GenerateMockPayload(Guid id, Payload payload = null)
        {
            if (payload == null)
            {
                payload = new Payload();
            }

            payload.OrderCode = id;
            payload.BrandType = "bob";

            return JsonConvert.SerializeObject(payload);
        }

        public static string GenerateMockPayloadWithInternalModel(Guid id)
        {
            var payload = new PayloadWithModel
            {
                TransportModel = new TransportModel
                {
                    Name = "hello world"
                }
            };

            return GenerateMockPayload(id, payload);
        }

        public static Mock<HttpMessageHandler> GetMockHandler(HttpContent stringContent = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                // Setup the PROTECTED method to mock
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                // prepare the expected response of the mocked http call
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = stringContent
                })
                .Verifiable();
            return handlerMock;
        }
    }
}
