using System;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    [Flags]
    public enum ReaderProvisionResult
    {
        None = 0,
        Created = 1,
        Deleted = 2,
        ReaderAlreadyExists = 4,
        DirectorIsBusy = 8,
        CreateFailed = 16,
        Updated = ReaderAlreadyExists | Created | Deleted,
        UpdateFailed = ReaderAlreadyExists | CreateFailed,
        DeleteFailed = ReaderAlreadyExists | Created | CreateFailed,
    }
}