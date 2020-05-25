using System;
using System.Threading.Tasks;
using CaptainHook.Api.Models;
using CaptainHook.Common;
using CaptainHook.Common.Remoting;
using Microsoft.AspNetCore.Authorization;
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
        /// <summary>
        /// Refreshes configuration for the given event
        /// </summary>
        /// <param name="request">Request with details to refresh configuration</param>
        /// <returns>If the event name is valid, returns its configuration. If invalid, returns BadRequest</returns>
        [HttpPost]
        public async Task<IActionResult> RefreshConfigForEvent([FromBody]RefreshConfigRequest request)
        {
            try
            {
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(new Uri(ServiceNaming.DirectorServiceFullName));
                await directorServiceClient.ReloadConfigurationForEventAsync(request.EventName);

                return Ok();
            }
            catch
            {
                return BadRequest();
            }
        }
    }
}