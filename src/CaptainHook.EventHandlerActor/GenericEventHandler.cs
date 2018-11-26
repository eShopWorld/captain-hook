namespace CaptainHook.EventHandlerActor
{
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Authentication;
    using Common.Telemetry;
    using Eshopworld.Core;

    public class GenericEventHandler : BaseHandler
    {
        private readonly HttpClient _client;
        private readonly IAccessTokenHandler _accessTokenHandler;
        private readonly IBigBrother _bigBrother;
        private readonly WebHookConfig _webHookConfig;

        public GenericEventHandler(
            IAccessTokenHandler accessTokenHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebHookConfig webHookConfig)
        {
            _client = client;
            _accessTokenHandler = accessTokenHandler;
            _bigBrother = bigBrother;
            _webHookConfig = webHookConfig;
        }

        public override async Task MakeCall(MessageData data)
        {
            //make a call to client identity provider
            if (_webHookConfig.RequiresAuth)
            {
                var token = await _accessTokenHandler.GetToken();
                _client.SetBearerToken(token);
            }

            //make a call to their api
            //todo polly 429 and 503
            var response1 = await _client.PostAsJsonAsync(_webHookConfig.Uri, data.Payload);

            //todo decisions, either continue to make calls or create a new event and throw it on to the appropriate topic for process
            if (response1.IsSuccessStatusCode)
            {
                //todo don't be sending customer payloads around for logging
                _bigBrother.Publish(new MessageExecuted(data.Handle, data.Type, data.Payload));
            }

            //todo add msg to the eda flow for a response, one way or another send the result to checkout
        }
    }

    public class WebHookConfig
    {
        public string Uri { get; set; }

        public bool RequiresAuth { get; set; }

        public AuthConfig AuthConfig { get; set; }
    }

    public interface IHandler
    {
        Task MakeCall(MessageData data);
    }

    public class HandlerConfig
    {
        public string Uri { get; set; }

        public string Payload { get; set; }
    }
}