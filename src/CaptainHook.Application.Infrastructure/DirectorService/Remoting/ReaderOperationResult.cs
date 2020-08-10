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

    public enum UpdateReaderResult
    {
        None = 0,
        Success,
        DoesNotExist,
        DirectorIsBusy,
        Failed
    }
}