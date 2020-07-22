using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Contract;
using Eshopworld.Core;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CaptainHook.Api.Controllers
{
    [Route("api/event")]
    [Authorize(Policy = AuthorisationPolicies.SubscribersManagement)]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IBigBrother _bigBrother;
        private readonly IMediator _mediator;

        [HttpPut]
        [Route("/{name-of-event}/subscriber/{name-of-subscriber}")]
        public async Task<IActionResult> PutSubscriber([FromBody] SubscriberDto dto)
        {




            return new StatusCodeResult(StatusCodes.Status418ImATeapot);
        }
    }
}