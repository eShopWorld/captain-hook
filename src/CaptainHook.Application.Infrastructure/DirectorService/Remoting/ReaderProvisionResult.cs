using System;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public enum ReaderProvisionResult
    {
        None = 0,
        Success,
        NoActionTaken,
        DirectorIsBusy,
        CreateFailed,
        UpdateFailed,
    }
}