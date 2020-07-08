using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.Contract;
using Eshopworld.Core;
using FluentValidation;
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
    [Authorize(Policy = AuthorisationPolicies.SubscribersAccess)]
    public class SubscribersController : ControllerBase
    {


        private readonly IBigBrother _bigBrother;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="bigBrother"></param>
        public SubscribersController(IBigBrother bigBrother)
        {
            _bigBrother = bigBrother;
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

        [HttpPost]
        public async Task<IActionResult> SaveSubscriberAsync([FromBody] SubscriberDto dto)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return Ok();
                }
                else
                {
                    return BadRequest();
                }
            }
            catch (Exception exception)
            {
                _bigBrother.Publish(exception);
                return BadRequest();
            }
        }
    }


    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty();
            RuleFor(x => x.EventName).Length(2, 10);
            RuleFor(x => x.Webhooks).NotNull().SetValidator(new WebhooksDtoValidator());
        }
    }

    public class WebhooksDtoValidator : AbstractValidator<WebhooksDto>
    {
        public WebhooksDtoValidator()
        {
            RuleFor(x => x.SelectionRule).MinimumLength(10).Must(p => p.StartsWith("sel"));
            RuleForEach(x => x.Endpoints).NotEmpty().SetValidator(new EndpointValidator());
        }
    }

    public class EndpointValidator : AbstractValidator<EndpointDto>
    {
        public EndpointValidator()
        {
            RuleFor(x => x.Selector).NotEmpty();
        }
    }
}