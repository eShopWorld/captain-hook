﻿using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Errors;
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
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutWebhook([FromRoute] string eventName, [FromRoute] string subscriberName, [FromBody] EndpointDto dto)
        {
            var request = new UpsertWebhookRequest(eventName, subscriberName, dto);
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                 error => error switch
                 {
                     ValidationError validationError => BadRequest(validationError),
                     DirectorServiceIsBusyError directorServiceIsBusyError => Conflict(directorServiceIsBusyError),
                     ReaderCreateError readerCreationError => UnprocessableEntity(readerCreationError),
                     CannotSaveEntityError cannotSaveEntityError => UnprocessableEntity(cannotSaveEntityError),
                     _ => StatusCode(StatusCodes.Status500InternalServerError, error) 
                 },
                 endpointDto => Created($"/{eventName}/subscriber/{subscriberName}/webhooks/endpoint/", endpointDto)
             );
        }

        /// <summary>
        /// Delete the default webhook for the provided event and subscriber
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <returns></returns>
        [HttpDelete("{eventName}/subscriber/{subscriberName}/webhooks/endpoint")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult>
            DeleteDefaultWebhook([FromRoute] string eventName, [FromRoute] string subscriberName) =>
            await DeleteWebhook(eventName, subscriberName, null);

        /// <summary>
        /// Delete a webhook for the provided event, subscriber and selector
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <param name="selector">Endpoint selector</param>
        /// <returns></returns>
        [HttpDelete("{eventName}/subscriber/{subscriberName}/webhooks/endpoint/{selector}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteWebhook([FromRoute] string eventName, [FromRoute] string subscriberName, [FromRoute] string selector)
        {
            var request = new DeleteWebhookRequest(eventName, subscriberName, selector);
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                error => error switch
                {
                    ValidationError validationError => BadRequest(validationError),
                    CannotRemoveLastEndpointFromSubscriberError cannotRemoveLast => BadRequest(cannotRemoveLast),
                    EndpointNotFoundInSubscriberError endpointNotFound => BadRequest(endpointNotFound),
                    EntityNotFoundError entityNotFoundError => NotFound(entityNotFoundError),
                    DirectorServiceIsBusyError directorServiceIsBusyError => Conflict(directorServiceIsBusyError),
                    ReaderDeleteError readerDeleteError => UnprocessableEntity(readerDeleteError),
                    ReaderDoesNotExistError readerDoesNotExistError => UnprocessableEntity(readerDoesNotExistError),
                    CannotUpdateEntityError cannotUpdateEntityError => StatusCode(StatusCodes.Status500InternalServerError, cannotUpdateEntityError),
                    _ => StatusCode(StatusCodes.Status500InternalServerError, error)
                },
                Ok
            );
        }

        /// <summary>
        /// Insert or update a subscriber
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <param name="dto">Webhook configuration</param>
        /// <returns></returns>
        [HttpPut("{eventName}/subscriber/{subscriberName}")]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutSuscriber([FromRoute] string eventName, [FromRoute] string subscriberName, [FromBody] SubscriberDto dto)
        {
            var request = new UpsertSubscriberRequest(eventName, subscriberName, dto);
            var result = await _mediator.Send(request);

            return result.Match<IActionResult>(
                 error => error switch
                 {
                     ValidationError validationError => BadRequest(validationError),
                     DirectorServiceIsBusyError directorServiceIsBusyError => Conflict(directorServiceIsBusyError),
                     ReaderCreateError readerCreationError => UnprocessableEntity(readerCreationError),
                     CannotSaveEntityError cannotSaveEntityError => UnprocessableEntity(cannotSaveEntityError),
                     _ => StatusCode(StatusCodes.Status500InternalServerError, error)
                 },
                 subscriberDto => Created($"/{eventName}/subscriber/{subscriberName}", subscriberDto)
             );
        }

        
    }
}