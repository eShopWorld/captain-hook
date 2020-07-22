using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using Eshopworld.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptainHook.Api.Controllers
{
    [Route("api/event")]
    [Authorize(Policy = AuthorisationPolicies.DefineSubscribers)]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBigBrother _bigBrother;

        public EventsController(IMediator mediator, IBigBrother bigBrother)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _bigBrother = bigBrother ?? throw new ArgumentNullException(nameof(bigBrother));
        }

        [HttpPut]
        [Route("/{eventName}/subscriber/{subscriberName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> PutSubscriber([FromRoute] string eventName, [FromRoute] string subscriberName, [FromBody] SubscriberDto dto)
        {
            var request = new UpsertSubscriberRequest(eventName, subscriberName, dto);
            //var result = await _mediator.Send(request);

            return new StatusCodeResult(StatusCodes.Status418ImATeapot);
        }
    }
}