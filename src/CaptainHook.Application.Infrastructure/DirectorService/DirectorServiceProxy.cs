using System.Threading.Tasks;
using CaptainHook.Application.Infrastructure.DirectorService.Remoting;
using CaptainHook.Common.Configuration;
using CaptainHook.Domain.Errors;
using CaptainHook.Domain.Results;

namespace CaptainHook.Application.Infrastructure.DirectorService
{
    public class DirectorServiceProxy : IDirectorServiceProxy
    {
        private readonly IDirectorServiceRemoting _directorService;

        public DirectorServiceProxy(IDirectorServiceRemoting directorService)
        {
            _directorService = directorService;
        }

        public async Task<OperationResult<SubscriberConfiguration>> CallDirectorServiceAsync(ReaderChangeBase request)
        {
            var createReaderResult = await _directorService.ApplyReaderChange(request);

            return createReaderResult switch
            {
                ReaderChangeResult.Success => request.Subscriber,
                ReaderChangeResult.NoChangeNeeded => request.Subscriber,
                ReaderChangeResult.CreateFailed => new ReaderCreateError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                ReaderChangeResult.DeleteFailed => new ReaderDeleteError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                ReaderChangeResult.DirectorIsBusy => new DirectorServiceIsBusyError(),
                ReaderChangeResult.ReaderDoesNotExist => new ReaderDoesNotExistError(request.Subscriber.EventType, request.Subscriber.SubscriberName),
                _ => new BusinessError("Director Service returned unknown result.")
            };
        }
    }
}