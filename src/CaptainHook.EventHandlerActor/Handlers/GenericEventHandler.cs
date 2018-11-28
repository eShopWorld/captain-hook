namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Common.Authentication;
    using Common.Telemetry;
    using Eshopworld.Core;

    public class GenericEventHandler : IHandler
    {
        private readonly HttpClient _client;
        private readonly IBigBrother _bigBrother;

        protected readonly WebHookConfig WebHookConfig;
        protected readonly IAccessTokenHandler AccessTokenHandler;

        public GenericEventHandler(
            IAccessTokenHandler accessTokenHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebHookConfig webHookConfig)
        {
            _client = client;
            AccessTokenHandler = accessTokenHandler;
            _bigBrother = bigBrother;
            WebHookConfig = webHookConfig;
        }

        public virtual Task Call<TRequest>(TRequest request)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task<TResponse> Call<TRequest, TResponse>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            //make a call to client identity provider
            if (WebHookConfig.RequiresAuth)
            {
                await AccessTokenHandler.GetToken(_client);
            }

            //make a call to their api
            //todo polly 429 and 503
            var response = await _client.PostAsJsonAsync(WebHookConfig.Uri, data.Payload);

            //todo decisions, either continue to make calls or create a new event and throw it on to the appropriate topic for process
            if (response.IsSuccessStatusCode)
            {
                //todo don't be sending customer payloads around for logging
                _bigBrother.Publish(new WebhookExecuted(data.Handle, data.Type, data.Payload));
            }
            else
            {
                //todo webhook failure
            }

            return response;
        }
    }
}