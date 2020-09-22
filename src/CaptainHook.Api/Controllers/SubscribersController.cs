using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using Eshopworld.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Subscribers controller
    /// </summary>
    [Route("api/subscribers")]
    [Authorize(Policy = AuthorisationPolicies.ReadSubscribers)]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="bigBrother">An instance of BigBrother logger</param>
        public SubscribersController(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
        }

        /// <summary>
        /// Retrieve all subscribers from current configuration
        /// </summary>
        /// <response code="200">Subscribers retrieved properly</response>
        /// <response code="503">Configuration has not been fully loaded yet</response>
        /// <response code="500">An error occurred while processing the request</response>
        /// <response code="401">Request not authorized</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var subscribers = await directorServiceClient.GetAllSubscribersAsync();

                if(subscribers.Count == 0)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                AuthenticationConfigSanitizer.Sanitize(subscribers.Values);

                return Ok(subscribers);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Get the configuration for the specified subscriber
        /// </summary>
        /// <param name="eventAndSubscriberNameKey">Event and subscriber name key</param>
        /// <returns>The configuration for the specified subscriber</returns>
        [Authorize(Policy = AuthorisationPolicies.ReadSubscribers)]
        [HttpGet("{eventAndSubscriberNameKey}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSubscriber([FromRoute] string eventAndSubscriberNameKey)
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var subscribers = await directorServiceClient.GetAllSubscribersAsync();

                if (subscribers.Count == 0)
                {
                    return StatusCode(StatusCodes.Status503ServiceUnavailable);
                }

                if (!subscribers.TryGetValue(eventAndSubscriberNameKey, out SubscriberConfiguration subscriberConfiguration))
                {
                    return NotFound();
                }

                AuthenticationConfigSanitizer.Sanitize(subscriberConfiguration);

                return Ok(subscriberConfiguration);
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}