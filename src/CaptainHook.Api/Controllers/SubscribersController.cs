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
        private readonly IMediator _mediator;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="mediator">An instance of MediatR mediator</param>
        /// <param name="bigBrother">An instance of BigBrother logger</param>
        public SubscribersController(IBigBrother bigBrother, IMediator mediator)
        {
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Retrieve all subscribers from current configuration
        /// </summary>
        /// <response code="200">Subscribers retrieved properly</response>
        /// <response code="503">Configuration has not been fully loaded yet</response>
        /// <response code="500">An error occurred while processing the request</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var subscribers = await directorServiceClient.GetAllSubscribersAsync();

                if(subscribers == null)
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
    }
}