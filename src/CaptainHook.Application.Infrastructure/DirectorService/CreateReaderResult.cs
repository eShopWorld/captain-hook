namespace CaptainHook.Application.Infrastructure.DirectorService
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