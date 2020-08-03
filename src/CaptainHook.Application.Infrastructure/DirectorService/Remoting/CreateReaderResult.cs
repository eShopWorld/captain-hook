namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public enum CreateReaderResult
    {
        None = 0,
        Created,
        AlreadyExists,
        DirectorIsBusy,
        Failed
    }
}