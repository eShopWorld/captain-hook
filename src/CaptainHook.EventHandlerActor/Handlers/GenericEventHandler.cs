namespace CaptainHook.EventHandlerActor.Handlers
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Authentication;
    using Common;
    using Common.Nasty;
    using Common.Telemetry;
    using Eshopworld.Core;

    public class GenericEventHandler : IHandler
    {
        private readonly HttpClient _client;
        protected readonly IBigBrother BigBrother;
        protected readonly WebHookConfig WebHookConfig;
        protected readonly IAuthHandler AuthHandler;

        public GenericEventHandler(
            IAuthHandler authHandler,
            IBigBrother bigBrother,
            HttpClient client,
            WebHookConfig webHookConfig)
        {
            _client = client;
            AuthHandler = authHandler;
            BigBrother = bigBrother;
            WebHookConfig = webHookConfig;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        public virtual async Task Call<TRequest>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            //make a call to client identity provider
            if (WebHookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }

            //call the platform something like 
            //call checkout
            var uri = WebHookConfig.Uri;

            if (data.Type == "checkout.domain.infrastructure.domainevents.retailerorderconfirmationdomainevent")
            {
                uri = $"https://checkout-api.ci.eshopworld.net/api/v2/webhook/PutOrderConfirmationResult/{data.CallbackPayload.OrderCode}";
                _client.SetBearerToken("eyJhbGciOiJSUzI1NiIsImtpZCI6IkQwQTM4OTU4RjlEMjFGQkE1RTQ3RDg3N0MxMTA3MkM5Q0MwQzdERUEiLCJ0eXAiOiJKV1QiLCJ4NXQiOiIwS09KV1BuU0g3cGVSOWgzd1JCeXljd01mZW8ifQ.eyJuYmYiOjE1NDUxMzExOTIsImV4cCI6MTU0NTEzNDc5MiwiaXNzIjoiaHR0cHM6Ly9zZWN1cml0eS1zdHMudGVzdC5lc2hvcHdvcmxkLm5ldCIsImF1ZCI6WyJodHRwczovL3NlY3VyaXR5LXN0cy50ZXN0LmVzaG9wd29ybGQubmV0L3Jlc291cmNlcyIsInRvb2xpbmcuZWRhLmFwaSJdLCJjbGllbnRfaWQiOiJ0b29saW5nLmVkYS5jbGllbnQiLCJzY29wZSI6WyJlZGEuYXBpLmFsbCJdfQ.C3E-Os9l_4J3fZra7QF-vfGWfMSI1Kj4UKX14fFevq1_ISoR0CfCS_NW0J25tPE8e3g8tNYJPDbh6E4Ox47Bu_RyeyhenpRagrVeQTx5x5FCPPniqU3kGoLLGIVndstvtPuiwaCjNorQCyxiNZ8Twmg1BJjNCmjn16kVBZoc0sUEydavfcTJyzUVGSVGlTka2Wy7AlTy_6APzJBe3dJjpoMPjmlJ_cAbeVkJ_ypv2Y8Vpg3xwuLYOUaM9dPVCyVNRbt9NgcV_6d2kplfq5niNhfOAjLqu9UU3afLhcADFK4cGjYDgz91BtFWHivzVYB8lM-rCMuTO7IzDtwC-YajVA");
            }

            if (data.Type == "checkout.domain.infrastructure.domainevents.platformordercreatedomainevent")
            {
                uri = $"https://checkout-api.ci.eshopworld.net/api/v2/PutCorePlatformOrderCreateResult/{data.CallbackPayload.OrderCode}";
            }

            var response = await _client.PostAsJsonReliability(uri, data, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="TRequest"></typeparam>
        /// <typeparam name="TResponse"></typeparam>
        /// <param name="request"></param>
        /// <returns></returns>
        [Obsolete]
        public virtual async Task<HttpResponseDto> Call<TRequest, TResponse>(TRequest request)
        {
            if (!(request is MessageData data))
            {
                throw new Exception("injected wrong implementation");
            }

            //make a call to client identity provider
            if (WebHookConfig.RequiresAuth)
            {
                await AuthHandler.GetToken(_client);
            }
            
            var response = await _client.PostAsJsonReliability(WebHookConfig.Uri, data, BigBrother);

            BigBrother.Publish(new WebhookEvent(data.Handle, data.Type, data.Payload, response.IsSuccessStatusCode.ToString()));

            var dto = new HttpResponseDto
            {
                Content = await response.Content.ReadAsStringAsync(),
                StatusCode = (int)response.StatusCode
            };

            return dto;
        }
    }
}