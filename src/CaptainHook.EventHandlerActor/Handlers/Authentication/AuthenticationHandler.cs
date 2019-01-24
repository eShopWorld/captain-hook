using System;
using IdentityModel.Client;

namespace CaptainHook.EventHandlerActor.Handlers.Authentication
{
    public abstract class AuthenticationHandler
    {
        protected void ReportTokenUpdateFailure(TokenResponse response)
        {
            if (!response.IsError)
            {
                return;
            }
            throw new Exception($"Unable to get access token from STS. Error = {response.ErrorDescription}");
        }
    }
}
