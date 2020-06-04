using System;

namespace CaptainHook.DirectorService
{
    public interface IDateTimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
