using System;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    [Flags]
    public enum ReaderRefreshResult
    {
        None = 0,
        Created = 1,
        Deleted = 2,
        ReaderAlreadyExists = 4,
        DirectorIsBusy = 8,
        Failure = 16,
        Updated = Created | Deleted | ReaderAlreadyExists,
    }
}