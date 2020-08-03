namespace CaptainHook.Application.Infrastructure.DirectorService.Remoting
{
    public enum CreateReaderResult
    {
        Unknown,
        Created,
        AlreadyExists,
        DirectorIsBusy,
        Failed
    }
}