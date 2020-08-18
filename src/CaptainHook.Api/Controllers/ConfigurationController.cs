using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common;
using Eshopworld.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Configuration controller
    /// </summary>
    [Route("api/config")]
    [Authorize(Policy = AuthorisationPolicies.ReadSubscribers)]
    public class ConfigurationController : ControllerBase
    {
        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="bigBrother">BigBrother instance</param>
        public ConfigurationController(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// Reloads configuration for Captain Hook
        /// </summary>
        /// <response code="202">Configuration reload has been requested</response>
        /// <response code="400">Configuration reload has not been requested due to an error</response>
        /// <response code="409">Configuration reload has not been requested as another reload is in progress</response>
        /// <response code="401">Request not authorized</response>
        [HttpPost("reload")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ReloadConfiguration()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var requestReloadConfigurationResult = await directorServiceClient.RequestReloadConfigurationAsync();

                return requestReloadConfigurationResult == RequestReloadConfigurationResult.ReloadStarted
                    ? (IActionResult)Accepted()
                    : Conflict();
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return BadRequest();
            }
        }

        /// <summary>
        /// Returns whether an existing configuration reload request is in progress
        /// </summary>
        /// <response code="200">Configuration reload is not in progress</response>
        /// <response code="202">Configuration reload in progress</response>
        /// <response code="400">Cannot get status due to an error</response>
        /// <response code="401">Request not authorized</response>
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetConfigurationStatus()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var result = await directorServiceClient.GetReloadConfigurationStatusAsync();
                return result switch
                {
                    ReloadConfigurationStatus.InProgress => Accepted(),
                    _ => Ok()
                };
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception.ToExceptionEvent());
                return BadRequest();
            }
        }
    }
}