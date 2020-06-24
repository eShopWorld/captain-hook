using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Common;
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

                AuthenticationConfigSanitizer.Sanitize(subscribers.Values);

                return Ok(subscribers);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                return BadRequest();
            }
        }
    }
}