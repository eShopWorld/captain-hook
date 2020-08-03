using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Results;
using Eshopworld.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptainHook.Api.Controllers
{
    /// <summary>
    /// Events controller
    /// </summary>
    [Route("api/event")]
    [Authorize(Policy = AuthorisationPolicies.DefineSubscribers)]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="mediator">An instance of MediatR mediator</param>
        /// <param name="bigBrother">An instance of BigBrother logger</param>
        public EventsController(IMediator mediator, IBigBrother bigBrother)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
        }

        /// <summary>
        /// Insert or update a web hook
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <param name="dto">Webhook configuration</param>
        /// <returns></returns>
        [HttpPut("{eventName}/subscriber/{subscriberName}/webhooks/endpoint/")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutWebhook([FromRoute] string eventName, [FromRoute] string subscriberName, [FromBody] EndpointDto dto)
        {
            var request = new UpsertWebhookRequest(eventName, subscriberName, dto);
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}