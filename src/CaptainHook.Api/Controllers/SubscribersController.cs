using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Common;
using CaptainHook.Common.Configuration;
using CaptainHook.Common.Remoting;
using CaptainHook.Contract;
using CaptainHook.Domain.Services;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using Eshopworld.Core;
using FluentValidation;
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
        private readonly SubscribersService _subscribersService;

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

        [HttpGet]
        [Route("getbyevent")]
        public async Task<IActionResult> GetByEvent([FromQuery] string eventName)
        {
            var subscribers = await _subscribersService.GetByEvent(eventName);

            return subscribers.Match<IActionResult>(
                error => BadRequest(error), 
                data => Ok(data)
            );
        }

        [HttpPost]
        public IActionResult SaveSubscriber([FromBody] SubscriberDto dto)
        {
            //var result = subscriberService.AddSubscriber(dto);
            //return Ok(result.Data);

            var result = _subscribersService.AddSubscriber(dto);

            if (result.IsError)
            {
                return BadRequest(result.Error);
            }
            else
            {
                return Ok(result.Data);
            }
        }
    }

    public class SubscriberDtoValidator : AbstractValidator<SubscriberDto>
    {
        public SubscriberDtoValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("foobar").WithMessage("Provide a Name, please!");
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