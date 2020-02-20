using System.Net;
using System.Net.Http;

namespace CaptainHook.EventHandlerActor.Handlers
{
    public static class HttpResponseMessageExtensions
    {
        /// <summary>
        /// define "delivery failure" for the EDA context
        /// </summary>
        /// <param name="response">response object</param>
        /// <returns>true if response indicates EDA delivery failure</returns>
        public static bool IsDeliveryFailure(this HttpResponseMessage response) => response !=null && ((response.StatusCode >= HttpStatusCode.InternalServerError && response.StatusCode <= HttpStatusCode.NetworkAuthenticationRequired) || response.StatusCode == HttpStatusCode.TooManyRequests);
    }
}
