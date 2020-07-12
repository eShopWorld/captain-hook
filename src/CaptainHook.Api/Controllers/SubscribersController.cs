using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.Contract;
using CaptainHook.Domain.Common;
using CaptainHook.Domain.Requests;
using CaptainHook.Domain.RequestValidators;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Eshopworld.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Subscribers controller
    /// </summary>
    [Route("api/subscribers")]
    [Authorize(Policy = AuthorisationPolicies.SubscribersAccess)]
    [ApiController]
    public class SubscribersController : ControllerBase
    {
        private readonly IBigBrother _bigBrother;
        private readonly IMediator _mediator;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bigBrother"></param>
        public SubscribersController(IBigBrother bigBrother, IMediator mediator)
        {
            _bigBrother = bigBrother;
            _mediator = mediator;
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

        [HttpGet]
        [Route("getbyevent")]
        public async Task<IActionResult> GetByEvent([FromQuery] string eventName)
        {
            var query = new GetSubscribersForEventQuery
            {
                Name = eventName,
            };

            var response = await _mediator.Send(query);

            return response.Match(
               error => HandleQueryError(error),
               data => Ok(data)
           );
        }

        private IActionResult HandleQueryError(ErrorBase error)
        {
            switch (error)
            {
                case EntityNotFoundError notFoundError:
                    return new NotFoundObjectResult(notFoundError);
                case BusinessError businessError:
                    return new BadRequestObjectResult(businessError);
                default:
                    return StatusCode(StatusCodes.Status418ImATeapot, error);
            }
        }

        [HttpPost]
        public async Task<IActionResult> SaveSubscriber([FromBody] SubscriberDto dto)
        {
            var request = new AddSubscriberRequest
            {
                Name = dto.Name,
                EventName = dto.EventName
            };

            var response = await _mediator.Send(request);

            return response.Match<IActionResult>(
                error => BadRequest(error),
                data => Ok(data)
            );
        }
    }
}