using System.Threading;

namespace CaptainHook.Tests.Web.WebHooks
{
    public class GenericWebhookHandlerTests
    {
        private readonly CancellationToken _cancellationToken;

        public GenericWebhookHandlerTests()
        {
            _cancellationToken = new CancellationToken();
        }

        //todo update test
        //[IsLayer0]
        //[Fact]
        //public async Task ExecuteHappyPath()
        //{
        //    var (messageData, metaData) = EventHandlerTestHelper.CreateMessageDataPayload();

        //    var config = new WebhookConfig
        //    {
        //        Uri = "http://localhost/webhook",
        //        HttpVerb = HttpVerb.Put,
        //        AuthenticationConfig = new AuthenticationConfig(),
        //        WebhookRequestRules = new List<WebhookRequestRule>
        //        {
        //           new WebhookRequestRule
        //           {
        //               Source = new ParserLocation
        //               {
        //                   Path = "OrderCode"
        //               },
        //               Destination = new ParserLocation
        //               {
        //                   Location = Location.Uri
        //               }
        //           }
        //        }
        //    };

        //    var mockHttp = new MockHttpMessageHandler();
        //    var webhookRequest = mockHttp.When(HttpMethod.Put, $"{config.Uri}/{metaData["OrderCode"]}")
        //        .WithContentType("application/json", messageData.Payload)
        //        .Respond(HttpStatusCode.OK, "application/json", string.Empty);

        //    var httpClients = new IndexDictionary<string, HttpClient> {{new Uri(config.Uri).Host, mockHttp.ToHttpClient()}};

        //    var genericWebhookHandler = new GenericWebhookHandler(
        //        new Mock<IAuthenticationHandlerFactory>().Object,
        //        new RequestBuilder(),
        //        new Mock<IBigBrother>().Object,
        //        httpClients,
        //        config);

        //    await genericWebhookHandler.CallAsync(messageData, new Dictionary<string, object>(), _cancellationToken);

        //    Assert.Equal(1, mockHttp.GetMatchCount(webhookRequest));
        //}
    }
}
