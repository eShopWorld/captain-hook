using System;
using System.Threading.Tasks;
using CaptainHook.Application.Gateways;
using CaptainHook.Common;
using CaptainHook.Common.Remoting;
using Eshopworld.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Refresh configuration controller
    /// </summary>
    [Route("api/refresh-config")]
    [Authorize]
    public class RefreshConfigController: ControllerBase
    {
        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// Initializes a new instance of this class
        /// </summary>
        /// <param name="bigBrother">BigBrother instance</param>
        public RefreshConfigController(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
        }

        /// <summary>
        /// Reloads configuration for Captain Hook
        /// </summary>
        /// <response code="202">Configuration reload has been requested</response>
        /// <response code="400">Configuration reload has not been requested due to an error</response>
        /// <response code="409">Configuration reload has not been requested as another reload is in progress</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ReloadConfiguration()
        {
            try
            {
                var directorServiceUri = new Uri(ServiceNaming.DirectorServiceFullName);
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(directorServiceUri);
                var requestReloadConfigurationResult = await directorServiceClient.RequestReloadConfigurationAsync();

                return requestReloadConfigurationResult == RequestReloadConfigurationResult.ReloadStarted
                    ? (IActionResult) Accepted()
                    : Conflict();
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                return BadRequest();
            }
        }
    }
}