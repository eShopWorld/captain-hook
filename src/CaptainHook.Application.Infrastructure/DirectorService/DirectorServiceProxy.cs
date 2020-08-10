﻿using System.Linq;
using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Application.Infrastructure.Mappers;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    public class DirectorServiceProxy : IDirectorServiceProxy
    {
        private readonly ISubscriberEntityToConfigurationMapper _entityToConfigurationMapper;
        private readonly IDirectorServiceRemoting _directorService;

        public DirectorServiceProxy(ISubscriberEntityToConfigurationMapper entityToConfigurationMapper, IDirectorServiceRemoting directorService)
        {
            _entityToConfigurationMapper = entityToConfigurationMapper;
            _directorService = directorService;
        }

        public async Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            var subscriberConfigs = await _entityToConfigurationMapper.MapSubscriber(subscriber);
            var createReaderResult = await _directorService.CreateOrUpdateReaderAsync(subscriberConfigs.Single());

            return createReaderResult switch
            {
                ReaderOperationResult.Success => true,
                ReaderOperationResult.AlreadyExists => true,
                ReaderOperationResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                ReaderOperationResult.Failed => new ReaderCreationError(subscriber),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}