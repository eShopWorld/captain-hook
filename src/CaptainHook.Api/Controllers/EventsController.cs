﻿using System;
using System.Threading.Tasks;
using CaptainHook.Api.Constants;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Application.Results;
using CaptainHook.Contract;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;
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

        /// <summary>
        /// Create an instance of this class
        /// </summary>
        /// <param name="mediator">An instance of MediatR mediator</param>
        public EventsController(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Insert or update a web hook
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <param name="selector">Endpoint selector, use * (asterisk) for the default endpoint</param>
        /// <param name="dto">Webhook configuration</param>
        /// <returns></returns>
        /// <response code="201">The requested webhook has been successfully created</response>
        /// <response code="400">A validation error has occurred</response>   
        /// <response code="401">This request requires authentication</response>   
        /// <response code="403">This request requires authorization</response>   
        /// <response code="409">Director service is currently busy and unable to process this request</response>   
        /// <response code="422">An error occurred either with Reader creation or while saving the entity</response>   
        /// <response code="500">The server encountered an unexpected error</response>  
        [HttpPut("{eventName}/subscriber/{subscriberName}/webhooks/endpoint/{selector}")]
        [ProducesResponseType(typeof(EndpointDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutWebhook([FromRoute] string eventName, [FromRoute] string subscriberName, [FromRoute] string selector, [FromBody] EndpointDto dto)
        {
            var request = new UpsertWebhookRequest(eventName, subscriberName, selector, dto);
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
                 endpointDto => Created($"/{eventName}/subscriber/{subscriberName}/webhooks/endpoint/{selector}", endpointDto)
             );
        }

        /// <summary>
        /// Delete a webhook for the provided event, subscriber and selector
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <param name="selector">Endpoint selector, use * (asterisk) for the default endpoint</param>
        /// <returns></returns>
        /// <response code="200">The requested webhook has been successfully deleted</response>
        /// <response code="400">A validation error has occurred</response>   
        /// <response code="401">This request requires authentication</response>   
        /// <response code="403">This request requires authorization</response>   
        /// <response code="409">Director service is currently busy and unable to process this request</response>   
        /// <response code="422">An error occurred either with Reader deletion/missing Reader or while updating the entity</response>   
        /// <response code="404">The requested webhook does not exist</response>   
        /// <response code="500">The server encountered an unexpected error</response>   
        [HttpDelete("{eventName}/subscriber/{subscriberName}/webhooks/endpoint/{selector}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(EntityNotFoundError), StatusCodes.Status404NotFound)]
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
        /// <response code="201">The requested subscriber has been successfully created</response>
        /// <response code="202">The requested subscriber has been successfully updated</response>   
        /// <response code="400">A validation error has occurred</response>   
        /// <response code="401">This request requires authentication</response>   
        /// <response code="403">This request requires authorization</response>   
        /// <response code="409">Director service is currently busy and unable to process this request</response>   
        /// <response code="422">An error occurred either with Reader creation or while saving the entity</response>   
        /// <response code="500">The server encountered an unexpected error</response>   
        [HttpPut("{eventName}/subscriber/{subscriberName}")]
        [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(SubscriberDto), StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutSubscriber([FromRoute] string eventName, [FromRoute] string subscriberName, [FromBody] SubscriberDto dto)
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
                    upsertResult => upsertResult.UpsertType switch
                    {
                        UpsertType.Created => Created($"/{eventName}/subscriber/{subscriberName}", upsertResult.Dto),
                        UpsertType.Updated => Accepted($"/{eventName}/subscriber/{subscriberName}", upsertResult.Dto),
                        _ => StatusCode(StatusCodes.Status500InternalServerError, "Operation unknown")
                    }
             );
        }

        /// <summary>
        /// Delete a subscriber  for the provided event and subscriber
        /// </summary>
        /// <param name="eventName">Event name</param>
        /// <param name="subscriberName">Subscriber name</param>
        /// <returns></returns>
        /// <response code="200">The requested subscriber has been successfully deleted</response>
        /// <response code="400">A validation error has occurred</response>   
        /// <response code="401">This request requires authentication</response>   
        /// <response code="403">This request requires authorization</response>   
        /// <response code="409">Director service is currently busy and unable to process this request</response>   
        /// <response code="422">An error occurred either with Reader deletion/missing Reader or while updating the entity</response>   
        /// <response code="404">The requested subscriber does not exist</response>   
        /// <response code="500">The server encountered an unexpected error</response>   
        [HttpDelete("{eventName}/subscriber/{subscriberName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationError), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(DirectorServiceIsBusyError), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status422UnprocessableEntity)]
        [ProducesResponseType(typeof(EntityNotFoundError), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorBase), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteSubscriber([FromRoute] string eventName, [FromRoute] string subscriberName)
        {
            var request = new DeleteSubscriberRequest(eventName, subscriberName);
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
    }
}