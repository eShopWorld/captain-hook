﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService;
using CaptainHook.Application.Requests.Subscribers;
using CaptainHook.Contract;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Repositories;
using CaptainHook.Domain.Results;
using CaptainHook.Domain.ValueObjects;
using MediatR;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class UpsertSubscriberRequestHandler : IRequestHandler<UpsertSubscriberRequest, OperationResult<SubscriberDto>>
    {
        private readonly ISubscriberRepository _subscriberRepository;
        private readonly IDirectorServiceProxy _directorService;

        public UpsertSubscriberRequestHandler(ISubscriberRepository subscriberRepository, IDirectorServiceProxy directorService)
        {
            _subscriberRepository = subscriberRepository ?? throw new ArgumentNullException(nameof(subscriberRepository));
            _directorService = directorService ?? throw new ArgumentNullException(nameof(directorService));
        }

        public async Task<OperationResult<SubscriberDto>> Handle(UpsertSubscriberRequest request, CancellationToken cancellationToken)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (!(existingItem.Error is EntityNotFoundError))
            {
                return new BusinessError("Updating subscribers not supported!");
            }

            var subscriber = MapRequestToEntity(request);

            var directorResult = await _directorService.CreateReaderAsync(subscriber);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.AddSubscriberAsync(subscriber);
            if (saveResult.IsError)
            {
                return saveResult.Error;
            }

            return request.Subscriber;
        }

        private static SubscriberEntity MapRequestToEntity(UpsertSubscriberRequest request)
        {
            var webhooks = new WebhooksEntity(
                request.Subscriber.Webhooks.SelectionRule,
                request.Subscriber.Webhooks.Endpoints?.Select(MapEndpointEntity) ?? Enumerable.Empty<EndpointEntity>());
            var subscriber = new SubscriberEntity(
                    request.SubscriberName,
                    new EventEntity(request.EventName))
                .AddWebhooks(webhooks);

            return subscriber;
        }

        private static EndpointEntity MapEndpointEntity(EndpointDto endpointDto)
        {
            var authenticationEntity = MapAuthentication(endpointDto.Authentication);
            var endpoint = new EndpointEntity(endpointDto.Uri, authenticationEntity, endpointDto.HttpVerb, endpointDto.Selector);

            return endpoint;
        }

        private static AuthenticationEntity MapAuthentication(AuthenticationDto authenticationDto)
        {
            return authenticationDto switch
            {
                BasicAuthenticationDto dto => new BasicAuthenticationEntity(dto.Username, dto.Password),
                OidcAuthenticationDto dto => new OidcAuthenticationEntity(dto.ClientId, dto.ClientSecretKeyName, dto.Uri, dto.Scopes?.ToArray()),
                _ => null,
            };
        }
    }
}