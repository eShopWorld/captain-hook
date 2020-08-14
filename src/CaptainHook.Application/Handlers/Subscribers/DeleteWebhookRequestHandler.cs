using System;
using System.Collections.Immutable;
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
using Eshopworld.Core;
using MediatR;
using Polly;

namespace CaptainHook.Application.Handlers.Subscribers
{
    public class DeleteWebhookRequestHandler : IRequestHandler<DeleteWebhookRequest, OperationResult<SubscriberDto>>
    {
        private static readonly TimeSpan[] DefaultRetrySleepDurations = {
            TimeSpan.FromSeconds(1.0),
            TimeSpan.FromSeconds(2.0),
        };

        private readonly ISubscriberRepository _subscriberRepository;

        private readonly IDirectorServiceProxy _directorService;

        private readonly IBigBrother _bigBrother;

        private readonly TimeSpan[] _retrySleepDurations;

        public DeleteWebhookRequestHandler(
            ISubscriberRepository subscriberRepository,
            IDirectorServiceProxy directorService,
            IBigBrother bigBrother,
            TimeSpan[] sleepDurations = null)
        {
            _subscriberRepository = subscriberRepository;
            _directorService = directorService;
            _bigBrother = bigBrother;
            _retrySleepDurations = sleepDurations ?? DefaultRetrySleepDurations;
        }

        public async Task<OperationResult<SubscriberDto>> Handle(DeleteWebhookRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var executionResult = await Policy
                    .HandleResult<OperationResult<SubscriberDto>>(result => result.Error is CannotUpdateEntityError)
                    .WaitAndRetryAsync(_retrySleepDurations)
                    .ExecuteAsync(() => DeleteEndpointAsync(request));

                return executionResult;
            }
            catch (Exception ex)
            {
                _bigBrother.Publish(ex.ToExceptionEvent());
                return new UnhandledExceptionError($"Error processing {nameof(UpsertWebhookRequest)}", ex);
            }
        }

        private async Task<OperationResult<SubscriberDto>> DeleteEndpointAsync(DeleteWebhookRequest request)
        {
            var subscriberId = new SubscriberId(request.EventName, request.SubscriberName);
            var existingItem = await _subscriberRepository.GetSubscriberAsync(subscriberId);

            if (existingItem.IsError)
            {
                return existingItem.Error;
            }

            var removeResult = existingItem.Data.RemoveWebhookEndpoint(request.Selector);
            if (removeResult.IsError)
            {
                return removeResult.Error;
            }

            var directorResult = await _directorService.UpdateReaderAsync(existingItem);
            if (directorResult.IsError)
            {
                return directorResult.Error;
            }

            var saveResult = await _subscriberRepository.UpdateSubscriberAsync(existingItem);
            if (saveResult.IsError)
            {
                return saveResult.Error;
            }

            return MapToDto(saveResult);
        }

        private SubscriberDto MapToDto(SubscriberEntity subscriber)
        {
            return new SubscriberDto
            {
                Webhooks = new WebhooksDto
                {
                    SelectionRule = subscriber.Webhooks.SelectionRule,
                    Endpoints = subscriber.Webhooks.Endpoints.Select(MapEndpointDto).ToList()
                }
            };
        }

        private EndpointDto MapEndpointDto(EndpointEntity endpointEntity)
        {
            return new EndpointDto
            {
                Selector = endpointEntity.Selector,
                Uri = endpointEntity.Uri,
                HttpVerb = endpointEntity.HttpVerb,
                UriTransform = new UriTransformDto { Replace = endpointEntity.UriTransform.Replace },
                Authentication = new AuthenticationDto
                {
                    Uri = endpointEntity.Authentication.Uri,
                    ClientId = endpointEntity.Authentication.ClientId,
                    Scopes = endpointEntity.Authentication.Scopes.ToList(),
                    Type = endpointEntity.Authentication.Type,
                    ClientSecret = new ClientSecretDto
                    {
                        Name = endpointEntity.Authentication.SecretStore.SecretName,
                        Vault = endpointEntity.Authentication.SecretStore.KeyVaultName
                    }
                }
            };
        }
    }
}