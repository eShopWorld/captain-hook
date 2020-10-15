namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public enum ReaderChangeResult
    {
        None = 0,
        Success,
        DirectorIsBusy,
        CreateFailed,
        DeleteFailed,
        ReaderDoesNotExist,
        NoChangeNeeded
    }
}