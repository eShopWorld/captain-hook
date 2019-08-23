using CaptainHook.Common.Authentication;
using CaptainHook.Common.Exceptions;
using IdentityModel.Client;
using Newtonsoft.Json;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public abstract class AuthenticationHandler
    {
        protected void ReportTokenUpdateFailure(OidcAuthenticationConfig config, TokenResponse response)
        {
            if (!response.IsError)
            {
                return;
            }

            throw new ClientTokenFailureException(response.Exception)
            {
                ClientId = config.ClientId,
                Scopes = string.Join(" ", config.Scopes),
                TokenType = response.TokenType,
                Uri = config.Uri,
                Error = response.Error,
                ErrorCode = response.HttpStatusCode,
                ErrorDescription = response.ErrorDescription,
                HttpErrorReason = response.HttpErrorReason,
                ResponsePayload = response.Json.ToString(Formatting.None)
            };
        }
    }
}
