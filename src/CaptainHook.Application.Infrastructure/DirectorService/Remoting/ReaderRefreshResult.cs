using System;

namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    [Flags]
    public enum ReaderRefreshResult
    {
        None = 0,
        Success = 1,
        ReaderExists = 2,
        DirectorIsBusy = 4,
        Failure = 8,
    }
}