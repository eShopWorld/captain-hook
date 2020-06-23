using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Common;
using CaptainHook.Common.Authentication;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using Eshopworld.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Api.Controllers
{
    [Route("api/subscribers")]
    [Authorize(Policy = AuthorisationPolicies.SubscribersAccess)]
    public class SubscribersController : ControllerBase
    {
        private const string SecretDataReplacementString = "***";

        private readonly IBigBrother _bigBrother;

        public SubscribersController(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// Retrieve all subscribers from current configuration
        /// </summary>
        /// <response code="200">Subscribers retrieved properly</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var subscribers = await directorServiceClient.GetAllSubscribersAsync();

                HideCredentials(subscribers.Values);

                return Ok(subscribers);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                return BadRequest();
            }
        }

        private void HideCredentials(IEnumerable<SubscriberConfiguration> subscribers)
        {
            foreach (var subscriber in subscribers)
            {
                HideWebhookCredentials(subscriber);
                HideWebhookCredentials(subscriber.Callback);

                var routes = subscriber.WebhookRequestRules.SelectMany(r => r.Routes);
                foreach (var route in routes)
                {
                    HideWebhookCredentials(route);
                }
            }
        }

        private void HideWebhookCredentials(WebhookConfig configuration)
        {
            if (configuration == null)
                return;

            switch (configuration.AuthenticationConfig)
            {
                case OidcAuthenticationConfig oidcAuth:
                    oidcAuth.ClientSecret = SecretDataReplacementString;
                    break;
                case BasicAuthenticationConfig basicAuth:
                    basicAuth.Password = SecretDataReplacementString;
                    break;
            }
        }
    }
}