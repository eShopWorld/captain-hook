using Eshopworld.Tests.Core;
using System.Net;
using System.Net.Http;
using Xunit;
using CaptainHook.EventHandlerActor.Handlers;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class HttpResponseMessageTests
    {
        [Theory, IsLayer0]
        [InlineData(HttpStatusCode.InternalServerError, true)]
        [InlineData(HttpStatusCode.NotImplemented, true)]
        [InlineData(HttpStatusCode.BadGateway, true)]
        [InlineData(HttpStatusCode.ServiceUnavailable, true)]
        [InlineData(HttpStatusCode.GatewayTimeout, true)]
        [InlineData(HttpStatusCode.HttpVersionNotSupported, true)]
        [InlineData(HttpStatusCode.VariantAlsoNegotiates, true)]
        [InlineData(HttpStatusCode.InsufficientStorage, true)]
        [InlineData(HttpStatusCode.LoopDetected, true)]
        [InlineData(HttpStatusCode.NotExtended, true)]
        [InlineData(HttpStatusCode.NetworkAuthenticationRequired, true)]
        [InlineData(HttpStatusCode.TooManyRequests, true)]
        [InlineData(HttpStatusCode.OK, false)]
        [InlineData(HttpStatusCode.NoContent, false)]
        [InlineData(HttpStatusCode.Created, false)]
        [InlineData(HttpStatusCode.BadRequest, false)]
        [InlineData(HttpStatusCode.NotModified, false)]
        public void IsDeliveryFailureAllPossibleCodesTests(HttpStatusCode input, bool expectedResult)
        {
            Assert.True(expectedResult == new HttpResponseMessage(input).IsDeliveryFailure());
        }
    }
}

