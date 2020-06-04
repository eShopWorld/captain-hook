﻿using System;
using System.Threading.Tasks;
using CaptainHook.Api.Models;
using CaptainHook.Common;
using CaptainHook.Common.Remoting;
using Eshopworld.Core;
using Eshopworld.Telemetry;
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
        /// Refreshes configuration for the given event
        /// </summary>
        /// <returns>If the event name is valid, returns its configuration. If invalid, returns BadRequest</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshConfigForEvent()
        {
            try
            {
                var directorServiceClient = ServiceProxy.Create<IDirectorServiceRemoting>(new Uri(ServiceNaming.DirectorServiceFullName));
                await directorServiceClient.ReloadConfigurationForEventAsync();

                return Ok();
            }
            catch(Exception exception)
            {
                _bigBrother.Publish(exception);
                return BadRequest();
            }
        }
    }
}