﻿using System.Threading.Tasks;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Gateways
{
    public interface IDirectorServiceGateway
    {
        /// <summary>
        /// Creates reader service for single webhook in Subscriber.
        /// </summary>
        /// <param name="subscriber">Subscriber entity defining ReaderService to be created</param>
        /// <returns>
        /// True if creation has been invoked
        /// False if reader already exists
        /// ReaderCreationError if reader creation failed
        /// DirectorServiceIsBusyError if DirectorService is performing other operation
        /// </returns>
        Task<OperationResult<bool>> CreateReader(SubscriberEntity subscriber);
    }
}
