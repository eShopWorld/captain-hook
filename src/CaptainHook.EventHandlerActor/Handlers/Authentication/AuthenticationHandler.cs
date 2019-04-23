using CaptainHook.Common.Authentication;
using CaptainHook.Common.Exceptions;
using IdentityModel.Client;

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
                Uri = config.Uri,
                Error = response.Error,
                ErrorCode = response.HttpStatusCode,
                ErrorDescription = response.ErrorDescription,
                HttpErrorReason = response.HttpErrorReason
            };
        }
    }
}
