using System.Threading.Tasks;
using CaptainHook.Domain.Entities;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    class DirectorServiceGateway : IDirectorServiceGateway
    {
        public Task<OperationResult<bool>> CreateReaderAsync(SubscriberEntity subscriber)
        {
            throw new System.NotImplementedException();
        }
    }
}